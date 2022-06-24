# BasePipe Readme

This is a pipe where we do not consider
1. Temperature
2. Height/Elevation
3. Entrance effects

The only thing we consider here is the pipe friction factor 
calculateed using Churchill Correlation.

## Design

The BasePipe class is a straight up modification of the MockPipe class
which takes from the CustomResistor Example.

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
to avoid even more confusion.


Also, i didn't do unit checks as thoughly here. But all units
must be taken as SI. Or it won't work. However, UnitConversions
in baseparameters should work okay.

I still need to test that though.

# Bibliography

<a id="frictionFactorApproximations">
[1]
Zeyu, Z., Junrui, C., Zhanbin, L., Zengguang, X., & Peng, L. (2020). Approximations of the Darcy–Weisbach friction factor in a vertical pipe with full flow regime. Water Supply, 20(4), 1321-1333.
</a>
