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

However, the function of which we find the root is also related to 

$$f*Re^2$$

So we don't actually have to find the friction factor directly but can
calculate $f*Re^2$.

The main dependency is changed when the jacobianObject is declared and
when we need to find the jacobians.

I need to make a new class implementing the jacobianObject interface
and change it accordingly in the IsothermalPipe class.

### Strategies to Deal with hydrostatic pressure increment or decrement.

Hydrostatic pressure increment is challenging. It means that
flow must go against a pressure drop or pressure gradient.

The theory here is simple: the hydrostatic pressure increase is
$\rho g z$. Where $\rho$ is the fluid density.

In terms of kinematic pressure, it is just gz. 

Now, z is the vertical height increase of the pipe. 

Given a pipe length L, and an angle from horizontal of $\theta$,

$$ z = L \sin \theta$$

Therefore, kinematic pressure increment is 

$$ gz = L g \sin \theta $$

This  becomes the kinematic hydrostatic pressure head.

In terms of code, we will just use the Math.Sin() method. It will take
in the double in terms of radians.

We can also add another property to the BaseParameters called 
inclineAngle. It will be of type Angle in EngineeringUnits.

I will then set the pressure drop to be not just

nodeAPressure - nodeBPressure

This assumes of course nodeAPressure is more than nodeBPresusre.

$$\Delta p = nodeAPressure - nodeBPressure - gz$$

If gz is zero, then it's the normal formula.
If gz > 0, we have incline, and net pressure drop driving flow
is less than the zero gradient.
If gz < 0, then we have decline, and net pressure drop driving flow
is more than zero gradient.

here's the code
In baseParameters:
```csharp

// next we also have angles as well

public Angle inclineAngle { get; set; } =
new Angle(0.0, AngleUnit.Degree);
```

And then in the BiasingBehavior class:

```csharp

Length pipeLength;
pipeLength = _bp.pipeLength;
double gz;
// of course g is 9.81 m/s^2
// we note that z = L sin \theta
gz = 9.81 * pipeLength.As(LengthUnit.SI) *
Math.Sin(_bp.inclineAngle.As(AngleUnit.Radian));

deltaP -= gz;
```

Only thing now is to test it.
I want to test for zero flow for an incline angle.

At pressure 1.45 $m^2/s^2$, the z height is pretty much

$$z = \frac{1.45 m^2 s^{-2}}{9.81 m s^{-2}}$$
$$z = 0.147808\ m$$

And the incline angle to give zero flow is:

$$\theta = \arcsin \frac{z}{L} = \arcsin \frac{0.147808}{10}$$
$$\theta = 0.01478 radians$$
$$\theta = 0.84691 degrees$$

So i will input 0.84691 degrees as the incline angle and hopefully
that will stop any flow coming in at 1.45 m^2/s^2.

## Design:

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

There were a few quirks when i started using this code however.
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
