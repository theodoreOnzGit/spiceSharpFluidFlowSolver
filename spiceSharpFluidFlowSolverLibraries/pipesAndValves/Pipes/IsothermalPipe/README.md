# IsothermalPipe Readme

This is a pipe where we do not consider
1. Temperature
2. Entrance effects

The only thing we consider here is the pipe friction factor 
calculateed using Churchill Correlation. Plus some height effects.

## Issues with BasePipe

The BasePipe class is a straight up modification of the MockPipe class
which takes from the CustomResistor Example.

It has been tested somewhat to produce the same results as the
Churchill friction factor correlation. So that for a pressure drop
of 1.45 $m^2/s^2$, we can get about 3600 kg/s of flow through a pipe
1m in diameter 10m in length, assuming water at 18C is the fluid.
No entrance effects are considered here.

The issue with basepipe first and foremost is the inability to handle 
flow near zero or equal to zero. Because then the friction factor
terms will go towards infinity. Also BasePipe is not designed to deal 
with reverse flow.

The second issue is an inability to extract simulation data. However,
that is more to do with the simulation class rather than the pipe 
classes. So this issue will not be discussed here.

The third issue is that we want BasePipe to be able to deal with 
hydrostatic pressure and height in general.

### Strategies to deal with the zero flow issue:

For the zero flow issue, it is apparent that the issue comes from
the friction factor tending toward zero.

Since I've already verified the original churchill Friction factor
class, I will use that as a baseline to now compare a new class of
jacobians and with which we can eliminate the flow set to zero 
issue.

The two places where the friction factor is actually used is where
we get the Bejan derivative with respect to Re. dBe/dRe. 

This particular derivatve makes use of the derivative

$$\frac{d (f*Re^2) }{d Re}$$

While the fanning friction factor is not well behaved close to zero,
$f*Re^2$ is relatively well behaved close to zero. So I will have to
multiply Re in manually in order to obtain the relevant values. 

The other place this particular derivative is used is during root finding. 





## Design:

Most of the math happens in biasing behavior, where a specific element
set is created called elements with the following indices:

```csharp
this._elements = new ElementSet<double>(state.Solver, new[] {
		new MatrixLocation(indexA, indexA),
		new MatrixLocation(indexA, indexB),
		new MatrixLocation(indexB, indexA),
		new MatrixLocation(indexB, indexB)
		}, new[] { indexA, indexB });
```

The first four elements refer to the four partial derivatives:


and the last two elemnts refer to the RHS vectors here:


Once those elements are loaded, the IBiasingBehavior.Load() method
calculates the derivatives and adds them to the Ymatrix elements
(Jacobian) and also the RHS vector elements.

```csharp
this._elements.Add(
		// Y-matrix
		dm_dPA, dm_dPB, minus_dm_dPA, minus_dm_dPB,
		// RHS-vector
		nodeARHSTerm, nodeBRHSTerm);
```

The RHS matrix term is as follows:


This is because from the Newton Raphson method, we are using:


I believe there is some mistake in the custom resistor example 
because:

```csharp

var c = Math.Pow(Math.Abs(v) / _bp.A, 1.0 / _bp.B);
if (isNegative){
	c = -c;
}


c -= g * v;
_elements.Add(
		// Y-matrix
		g, -g, -g, g,
		// RHS-vector
		c, -c);
```

here, both the current c and the derivative g are guaranteed
positive values. Current outflow is positive based on the
convention used in spiceSharp's modified nodal analysis.

Therefore the correct RHS calculating code for Node A should be:

```csharp
double nodeA_RHS = -c + g*_nodeA.Value + (-g) * _nodeB.Value;
double nodeB_RHS = c + (-g)* _nodeA.Value + g * _nodeB.Value;
```

or equivalently,
```csharp
var v = _nodeA.Value - _nodeB.Value;
double nodeA_RHS = -c + g*v;
double nodeB_RHS = c - g*v;
```

An example of more correct code can be seen in the diodeBiasing 
csharp file

```csharp
_elements = new ElementSet<double>(biasingState.Solver,
		new MatrixLocation[]
		{
		// The Y-matrix elements
		new MatrixLocation(rowA, rowA),
		new MatrixLocation(rowA, rowB),
		new MatrixLocation(rowB, rowA),
		new MatrixLocation(rowB, rowB)
		},
		new int[] {
		// The right-hand side vector elements
		rowA,
		rowB
		});

/// <summary>
/// Loads the Y-matrix and right-hand side vector.
/// </summary>
public void Load()
{
	// Let us calculate the derivatives and the current
	double voltage = _variableA.Value - _variableB.Value;
	double current = Parameters.Iss * (Math.Exp(voltage / Vte) - 1.0);
	double derivative = current / Vte;

	// Load the Y-matrix and RHS vector
	double rhs = current - voltage * derivative;
	_elements.Add(
			// Y-matrix contributions
			derivative, -derivative,
			-derivative, derivative,
			// RHS vector contributions
			-rhs, rhs);
}
```

We can see here the jacobians are loaded correctly, where in nodeA,
the current value should be negative, since current flowing out
of NodeA is negative, but this value is subtracted from the jacobian
term in the RHS vector. In the same equation, the voltage and derivative
term should be positive.

And for node B, current value is positive since current going into
the node is negative, and having a minus sign infront of the 
current balance is also negative. They cancel out to become positive.
And the jacobian term times voltage becomes negative.

To avoid ambiguity, I spell it out for the user:


```csharp
double nodeARHSTerm;
nodeARHSTerm = -massFlowRateValue + dm_dPA * _nodeA.Value +
dm_dPB * _nodeB.Value;

double nodeBRHSTerm;
nodeBRHSTerm = massFlowRateValue + minus_dm_dPA * _nodeA.Value +
minus_dm_dPB * _nodeB.Value;
```

In future iterations, i might just name the variables PA and PB
to avoid even more confusion. Since nodeB.Value is essentially
the kinematic pressure of nodeB.


Also, i didn't do unit checks as thoughly here. But all units
must be taken as SI. Or it won't work. However, UnitConversions
in baseparameters should work okay.

I still need to test that though.

## inner workings and bugs

Most work happens in the BiasingBehavior class.

```csharp
private readonly IVariable<double> _nodeA, _nodeB;
private readonly ElementSet<double> _elements;
private readonly BaseParameters _bp;
private readonly BiasingParameters _baseConfig;
private IFrictionFactorJacobian _jacobianObject;
```

In that class, a list of variables is instantiated.

Of these, we pay attention to BaseParameters class which
contains the properties of the pipe and fluid within.

Also of importance here are elementSet classes, which
contain the elements of the matrices we want to add
to when we do the nodal analysis and biasing.

However, the bulk of the work happnes in the jacobian
Object, which helps us calculate the jacobian as spelled
out in the pipe theory readme.

In the constructor
```csharp

// Construct the IFrictionFactorJacobian object
_jacobianObject = new ChurchillFrictionFactorJacobian();
```

We construct our friction factor object. No dependency
injection here as i don't want to overcomplicate things
yet.

To help calculate our jacobians, the following code
is executed:

```csharp
double dm_dPA = _jacobianObject.dm_dPA(crossSectionalArea,
		fluidViscosity,
		hydraulicDiameter,
		pressureDrop,
		absoluteRoughness,
		pipeLength,
		fluidKinViscosity);

double dm_dPB = _jacobianObject.dm_dPB(crossSectionalArea,
		fluidViscosity,
		hydraulicDiameter,
		pressureDrop,
		absoluteRoughness,
		pipeLength,
		fluidKinViscosity);

double minus_dm_dPA = -_jacobianObject.dm_dPA(crossSectionalArea,
		fluidViscosity,
		hydraulicDiameter,
		pressureDrop,
		absoluteRoughness,
		pipeLength,
		fluidKinViscosity);

double minus_dm_dPB = -_jacobianObject.dm_dPB(crossSectionalArea,
		fluidViscosity,
		hydraulicDiameter,
		pressureDrop,
		absoluteRoughness,
		pipeLength,
		fluidKinViscosity);
```
The above required parameters are pulled straight from base parameters
prior to executing this code.

```csharp

Length pipeLength;
pipeLength = _bp.pipeLength;

KinematicViscosity fluidKinViscosity;
fluidKinViscosity = _bp.fluidKinViscosity;
```

All of these use the EngineeringUnits package inherited from
SharpFluids:

```csharp
using EngineeringUnits;
using EngineeringUnits.Units;
```
In this project, dimensionless quantites are doubles,
the rest are all dimensioned accordingly to force unit checks.

However, when it comes time for the final conversion,
only then are the Dimensioned Unit objects converted into doubles
and the unit will always be SI.

```csharp

MassFlow massFlowRate;
// i noticed that dmdRe is the same
// as mass/Re due to its linear relationship
massFlowRate = _jacobianObject.dmdRe(
		crossSectionalArea,
		fluidViscosity,
		hydraulicDiameter);
massFlowRate *= Re;

double massFlowRateValue;
massFlowRateValue = massFlowRate.As(MassFlowUnit.SI);
```

There were a few queirks when i started using this code however.
Mainly, the flowrate starts out at zero. Now when it does that
Reynold's number is zero. Which is okay. However, the friction
factor from churchill correlation will be infinite. Because
the fanning friction factor is:

$$\frac{16}{Re}$$

So since the friction factor goes to infinity, the code throws
and error. And also, the gradient when the flowrate approaches 
zero is undefined.

However, the pressure drop is not undefined, it just approaches
zero and is therefore a well behaved function. For this reason,
it may be prudent to use an analytical function or at least 
modify the jacobian equations slightly to ensure that it doesn't
approach zero. 

The pressure drop goes as:

$$\frac{16}{Re} * Re^2 = 16 * Re$$

I might want to use a different implmentation of this in future.
This is so that I will have a mathematically well behaved jacobian.


The rest of the code though, will be documented through its underlying jacobian
functions.






# Bibliography

<a id="frictionFactorApproximations">
[1]
Zeyu, Z., Junrui, C., Zhanbin, L., Zengguang, X., & Peng, L. (2020). Approximations of the Darcy–Weisbach friction factor in a vertical pipe with full flow regime. Water Supply, 20(4), 1321-1333.
</a>
