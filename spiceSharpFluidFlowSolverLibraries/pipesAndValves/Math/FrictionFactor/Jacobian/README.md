# Jacobian Readme


## Purpose

The jacobian set of classes is meant to calculate the jacobian
terms:

$$\frac{\partial \dot{m}_{pipe} (kg/s)}{\partial p_A (m^2/s^2)}$$
$$\frac{\partial \dot{m}_{pipe}}{\partial p_B}$$

And likewise to node b,
$$-\frac{\partial \dot{m}_{pipe}}{\partial p_A}$$
$$-\frac{\partial \dot{m}_{pipe}}{\partial p_B}$$

## Methodology
From chain rule we can obtain:
$$\frac{d \Delta p}{d Re} =  \frac{d \Delta p}{d p_A}* 
\frac{d p_A}{d \dot{m}_{pipe}} * \frac{d \dot{m}_{pipe}}{d Re} $$

$$\frac{d \dot{m}_{pipe}}{d p_A} = 
\frac{\frac{d \Delta p}{d p_A} 
\frac{d \dot{m}_{pipe}}{d Re}}{\frac{d \Delta p}{d Re}}$$

The top two derivatives are easy to obtain analytically.

What remains is the bottom derivative.

I wish to obtain the derivative using nondimensional numbers
as far as possible. And when dimensional numbers are given,
I want to use the EngineeringUnits package to dimensionalise
quantites properly.

$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} 
* \frac{d \Delta p}{d (Re)}$$

To obtain the $\frac{d(Be)}{d \Delta p}$:

$$Be = \frac{\Delta p L^2}{\nu^2}$$
$$ \frac{d(Be)}{d \Delta p} = \frac{L^2}{\nu^2}$$


With this we can now obtain the derivative 
$\frac{d \Delta p}{d (Re)}$:

$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} 
* \frac{d \Delta p}{d (Re)}$$


$$\frac{d(Be)}{d(Re)}  = \frac{L^2}{\nu^2}
* \frac{d \Delta p}{d (Re)}$$

$$\frac{d \Delta p}{d (Re)} = \frac{d(Be)}{d(Re)} 
* \frac{\nu^2}{L^2}$$

### Numerical Derivative

To obtain the numerical derivative $\frac{d(Be)}{d(Re)}$:
$$f_{fanning} (Re)* Re^2 = \frac{32 Be}{ (\frac{4L}{D})^3 }$$
$$\frac{d(Be)}{d(Re)} = \frac{(\frac{4L}{D})^3}{32}
\frac{d}{d(Re)} ( f_{fanning}(Re)*Re^2 ) $$

Now we have our equation being fully nondimensional.
From this we can find the differential.
This of course assumes that  roughness ratio is constant.

The derivative can be achieved by direct numerical estimation of
the derivative. This should be the most straightforward way.
I have little in the way of testing the validity however.

Nevertheless, I can simply copy the numerical code that performs 
the calculations for fanning derivatives and add in  $Re^2$ to the 
function.

```csharp
```

The only test to do here is an internal consistency check.
But given that it's so simple, i'd rather forgo it.

Unless errors come forth later on.

### dpdRe dimensioned derivative
Now we just need to calculate:
$$\frac{d \Delta p}{d (Re)} = \frac{d(Be)}{d(Re)} 
* \frac{\nu^2}{L^2}$$

```csharp
```


It is also very important to our computation algorithms
that one pressure drop value corresponds to one Re value.

So we don't have the problem of one friction factor value
corresponding to two Re values.
$$\frac{d(Be)}{d(Re)}  = \frac{d(Be)}{d \Delta p} * \frac{d \Delta p}{d (Re)}
$$

I can't really test too much on this, except to copy/paste
the inner working code and see if it works as intended.

Also unit checks.

##  dmdRe dimensioned derivative
$$\frac{d \dot{m}_{pipe}}{d p_A} = 
\frac{\frac{d \Delta p}{d p_A} 
\frac{d \dot{m}_{pipe}}{d Re}}{\frac{d \Delta p}{d Re}}$$


Now we have settled the denominator, we can look at the numerator

dmdRe is in the numerator, and it has units of mass flowrate.


$$Re_D = \frac{\dot{m}_{pipe}D_{pipe}}{A_{xs}\mu_{fluid}}$$

$$\dot{m}_{pipe} = \frac{Re_{D} A_{xs} \mu_{fluid}}{D_{pipe}}$$

so we have the derivative 

$$\frac{d\dot{m}_{pipe}}{d(Re)} = \frac{A_{xs} 
\mu_{fluid}}{D_{pipe}}$$

Only three parameters are required: 


1. pipe cross sectional area
2. fluid viscosity
3. pipe hydraulic diameter

the return type will be dimensioned in mass flowrate


```csharp
```

##  $\partial \Delta p / \partial p_A$ and  $\partial \Delta p / \partial p_B$ derivatives

We note:

$$\Delta p = p_A - p_B$$

We go by the potential difference convention for circuits rather
than pressure drop which has inital - final.

From this we note:

$$\frac{\partial \Delta p}{\partial p_A} = 1$$
$$\frac{\partial \Delta p}{\partial p_B} = -1$$

For the sake of having such functions explicitly declared
in code, i'm making functions for these:

```csharp
```

May be trivial, but at least it makes code somewhat easier to read.
If one understands these derivations that is.

## dm/dp jacobian

Now this derivative here will be in some 
weird unit: $(kg/s)/(m^2/s^2)$

I know not of any existing unit being of this sort of unit.
But no matter, the final return type is double, since
this is the type expected to go into load.

I expect units to all be SI converted. So I'd like to convert them
in my functions before anything else.

So far, the jacobian can be calculated if the Reynold's number is 
known.

If the Reynold's number is not known, we have to guess it from 
the pressure drop.

For that, we need to convert the kinematic pressure drop to Bejan 
number, then obtain the Reynold's number given the relative
roughness and lengthToDiameter Ratio.

The code that is responsible for this is the Churchill friction
factor code. However, the jacobian code also inherits from the
Churchill friction factor code and will have the capability
to use the same functions.

```csharp

double pressureDropRoot(double Re){

	// fanning term
	//
	double fanningTerm;
	fanningTerm = this.fanning(Re, this.roughnessRatio);
	fanningTerm *= Math.Pow(Re,2.0);


	//  BejanTerm
	//
	double bejanTerm;
	bejanTerm = 32.0 * this.bejanNumber;
	bejanTerm *= Math.Pow(4.0*this.lengthToDiameter,-3);

	// to set this to zero, we need:
	//
	return fanningTerm - bejanTerm;

}

double ReynoldsNumber;
ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 1, 1e8);

// once I'm done, i want to clean up all terms
this.roughnessRatio = 0.0;
this.lengthToDiameter = 0.0;
this.bejanNumber = 0.0;


// then let's return Re

return ReynoldsNumber;
}
```
The function is called pressureDropRoots which guesses 
a Reynold's number from a given Bejan number.

However, i want my lengthToDiameter ratio and my 
roughnessRatio to be constant before i load the single
input single output delegate into the root finding algorithm
To do that, I assign the input parameters to variables
or properties within the class. These are then referenced 
by the delegate when the root finding occurs.

The FindRoots function in the mathnet library is then used
to find the Reynold's number.

Once that is done, the roughness ratio, bejan number and
lengthToDiameter ratio of the object are all set to zero.

Note that this is not thread safe.

Only after cleanup, then Reynold's number is returned.

Once the appropriate Reynold's Number is found, we can
then find the slope at that particular Re.

This is done from the ChurchillMathNetDerivative
class.

First the derivative dBe/dRe is calculated

```csharp
public double dB_dRe(double Re, double roughnessRatio,
		double lengthToDiameter){

	double lengthToDiameterTerm;
	lengthToDiameterTerm = Math.Pow(4.0*lengthToDiameter,
			3.0);


	//
	// firstly i need to use the derivative object

	IDerivative derivativeObj;
	derivativeObj = new MathNetDerivatives();

	// secondly i need a function with a double
	// in and out
	// this is the function that returns the Reynold's number
	// however, the roughness ratio is kept constant

	this.roughnessRatio = roughnessRatio;

	double constantRoughnessFanningReSq(double Re){

		double fanningReSq = this.fanning(Re, this.roughnessRatio)*
			Math.Pow(Re,2.0);

		return fanningReSq;
	}

	// now let's calculate the derivative at a specific Re

	double derivativeResult;
	derivativeResult = derivativeObj.calc(
			constantRoughnessFanningReSq, Re);

	// after i'm done, clean up the roughness ratio
	// variable within the class

	this.roughnessRatio = 0.0;


	derivativeResult *= lengthToDiameterTerm;
	derivativeResult /= 32.0;

	return derivativeResult;


}
```

This is important because we need to use this to find the
other derivatives. Now, do note that there is an issue
with this code here: If Re = 0, we will get some form
of infinity multiplied by zero in the intermediate 
calculation step. This might be patched up in future 
iterations.

Once dBe/dRe is found, the rest is simpler.

```csharp

public SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
		double lengthToDiameter,
		Length lengthScale,
		KinematicViscosity nu){

	lengthScale = lengthScale.ToUnit(LengthUnit.SI);
	nu = nu.ToUnit(KinematicViscosityUnit.SI);
	// dDeltaP_dRe will be in specific energy
	// SI unit is: m^2/s^2 
	// this is the same unit as kinematic pressure
	SpecificEnergy derivativeResult;

	// the type will be unknown unit
	var intermediateUnitResult = nu.Pow(2)/lengthScale.Pow(2);
	intermediateUnitResult *= this.dB_dRe(Re,roughnessRatio,
			lengthToDiameter);

	// after which we transform it to a base unit
	derivativeResult = (SpecificEnergy)intermediateUnitResult;

	return derivativeResult;
}

```
Note that the derivatives here are dimensioned using the 
EngineeringUnits package.

```csharp

public double dm_dPA(Area crossSectionalArea,
		DynamicViscosity fluidViscosity,
		Length hydraulicDiameter,
		double Re,
		double roughnessRatio,
		Length pipeLength,
		KinematicViscosity fluidKinViscosity){

	double derivativeResult;
	derivativeResult = this.dDeltaP_dPA();

	MassFlow dmdRe = this.dmdRe(crossSectionalArea,
			fluidViscosity,
			hydraulicDiameter);

	double lengthToDiameter;
	lengthToDiameter = pipeLength.As(LengthUnit.SI)/
		hydraulicDiameter.As(LengthUnit.SI);

	SpecificEnergy dDeltaP_dRe = this.dDeltaP_dRe(Re, 
			roughnessRatio,
			lengthToDiameter,
			pipeLength,
			fluidKinViscosity);

	derivativeResult *= dmdRe.As(MassFlowUnit.SI);
	derivativeResult /= dDeltaP_dRe.As(MassFlowUnit.SI);

	return derivativeResult;
}
```

Other such functions were similarly designed. 

And that's pretty much how the jacobian was designed.

More iterations are possible and may be used in future instances.

# Patches to Jacobian

## problem statement for patch 1
Now we note an issue where the friction factor term produces NaN values
when the pressure drop is zero. This is rather problematic for generating
our jacobian values especially since our initial pressure drop will be zero.

Furthermore, the jacobian term will not be able to deal with negative Reynold's
number should the flow reverse.

For the friction factor going close to zero, we recognise that the laminar
formula for friction factor, 16/Re for fanning friction factor will go to
infinity if Re goes to zero.

However, pressure drop doesn't go to infinity, but rather to zero.

It appears the quantity of interest to calculate pressure drop is not f,
but rather $f*Re^2$. $f*Re^2$ is a mathematically well behaved which does
not go to infinity at Re = 0. And for the laminar region, the formula to 
calculate this is 16Re. 

So to obtain our jacobian, we need to modify only two functions, 

1. the getRe function
2. the dB_dRe function

The getRe function has this major change in mind when it comes to finding
the Re from pressure drop. 

```csharp

double pressureDropRoot(double Re){

	// fanning term
	//
	//
	// Now here is a potential issue for stability,
	// if Re = 0, the fanning friction factor is not well behaved,
	// Hence it's better to use the laminar term at low Reynold's number
	//
	// we note that in the laminar regime, 
	// f = 16/Re
	// so f*Re^2 = 16*Re
	double fanningTerm;

	if (Re > 1800)
	{
		fanningTerm = this.fanning(Re, this.roughnessRatio);
		fanningTerm *= Math.Pow(Re,2.0);
	}
	else
	{
		fanningTerm = 16.0*Re;
	}


	//  BejanTerm
	//
	double bejanTerm;
	bejanTerm = 32.0 * this.bejanNumber;
	bejanTerm *= Math.Pow(4.0*this.lengthToDiameter,-3);

	// to set this to zero, we need:
	//
	return fanningTerm - bejanTerm;

}
```

So basically, we note that for Re below 1, the friction factor kind 
of goes to infinity. 
Hence we are better off using 16Re for the fanningTerm for Re below 1
. However, we note 
that using churchill correlation to calculate the bejan number in 
the laminar region is 
computationally expensive. We may as well use the 16Re for the 
fanningTerm in the entire
laminar region. That is Re<2300.

However, this presents another issue: there is potential 
discontinuity in transitioning
from the laminar Re equation and the churchill equation. Hence i'd 
rather not transition
at Re = 2300, but maybe at 1800 where the curve is smoother and the 
discontinuity is less.

I noted in earlier unit tests that there is excellent agreement for 
churchill and the laminar
friction factor equation at Re<2000. One can hope that at Re<2000, 
the discontinuity is small and we can still live with a smooth 
solver. 

One important test for this is to test for root finding at Re=1800,
and gradient finding at Re=1800. I suspect gradient finding at
Re=1800 will have issues.


Speaking of gradient finding, here is the new code for dB_dRe

```csharp

double constantRoughnessFanningReSq(double Re){

	// now the graph in the negative x direction should be a 
	// reflection of the graph in positive x direction along
	// the y axis. So this is using absolute value of Re

	Re = Math.Abs(Re);

	// the fanning friction factor function (this.fanning)
	// can return Re values for various values in turbulent as
	// well as laminar region
	//
	// However, if Re is close to zero, 
	// the function is not well behaved
	//
	// since we are returning f*Re^2
	// we can use the laminar region fanning friction factor
	// which is 16/Re
	// for lower Re eg. Re<1
	// However, we also note that it's quite computationally cheap
	// in that you only need to perform one calculation
	// Hence, it's quite advantageous to let it take more Reynold's 
	// numbers
	// so for most of the laminar regime, it is good to use the 
	// 16/Re formula
	//
	// We should note however that for a piecewise function
	// there is some discontinuity between the two functions
	// ie the churchill and the Pousille function
	//
	// While this is a concern, let's ignore it for now
	// and fix the problem if it crops up.
	//
	// So if Re>1800 we return the traditional fanning formula

	if (Re > 1800)
	{
		double fanningReSq = this.fanning(Re, this.roughnessRatio)*
			Math.Pow(Re,2.0);

		return fanningReSq;
	}

	// otherwise we return 16/Re*Re^2 or 16*Re
	//

	return 16.0*Re;

	// my only concern here is a potential problem if the root is exactly at
	// Re = 1800
}

```

It will be important to test dB_dRe at Re=1800. The way to test this is with the
non stablised version and see how bad the discontinuity is.

```zsh

  Failed tests.FrictionFactorTests.continuityTest_dB_dRe(Re: 1800) [< 1 ms]
  Error Message:
   Assert.Equal() Failure
Expected: 32036.447387421504
Actual:   88767.44732959196
```

so in other words, pretty bad...

```zsh

  Error Message:
   Assert.Equal() Failure
Expected: 32035.72572208941
Actual:   32000
```
For Re = 1799, the 16/Re function is compared against the traditional churchill
function, it seems that the discontinuity there isn't that bad.


The third way i guess to really deal with this discontinuity is to multiply
$Re^2$ in. However, that will do nothing really because of the log term,
which is also not well behaved close to zero.

There is also a way to patch the second way though: it is with linear 
interpolation or some other interpolation methods which smooth the curve.

The only criteria for this is that:

1. the function is not disjointed (continuous)
2. the function is still a function (one x value for one y value).

The function may not be smooth, but we only need the derivatives to find 
our roots of the equation so as to solve the hydraulic circuit. 
Nothing more. 

Hence we can use mathnet's interpolation functions.

For the $f(Re^2)$ function, i can supply two points. At Re = 0, this 
function's value is zero. At f(Re^2) = 1800, we will use the churchill
notation. Linearly interpolate and that's it!

This is convenient because the function in the laminar region
$f(Re^2)$ is very much linear: 16Re for the fanning friction
factor version.

with linear interpolation for the jacobian we get:

```csharp

double transitionPoint = 1800.0;

if (Re > transitionPoint)
{
	double fanningReSq = this.fanning(Re, this.roughnessRatio)*
		Math.Pow(Re,2.0);

	return fanningReSq;
}

// otherwise we return 16/Re*Re^2 or 16*Re
//
IInterpolation _linear;

IList<double> xValues = new List<double>();
IList<double> yValues = new List<double>();
xValues.Add(0.0);
xValues.Add(transitionPoint);

yValues.Add(0.0);
yValues.Add(this.fanning(transitionPoint,this.roughnessRatio)*
		Math.Pow(transitionPoint,2.0));

_linear = Interpolate.Linear(xValues,yValues);


return _linear.Interpolate(Re);
```

this yields better results at the transition point and also passes the
Re = 0  test.

```zsh

  Failed tests.FrictionFactorTests.continuityTest_dB_dRe(Re: 1800) [< 1 ms]
  Error Message:
   Assert.Equal() Failure
Expected: 32036.447387421504
Actual:   32018.719154875726

  Failed tests.FrictionFactorTests.continuityTest_dB_dRe(Re: 1799) [8 ms]
  Error Message:
   Assert.Equal() Failure
Expected: 32035.72572208941
Actual:   32000.98522962071
```
While not exactly equal, the linearly interpolated result is quite close to
the churchill value at the transition Point. 

The differences in derivative aren't great and within 0.2% of the churchill 
derivative. That's fine by me. As long as we can find the roots and solve
the hydraulics equation, we don't have to be extremely precise about 
the derivative and smooth functions. 

And well, the same linear interpolation process was done for the 
pressureDropRoot nested function within the ChurchillFrictionFactor
class.


```csharp

double pressureDropRoot(double Re){

	// fanning term
	//
	//
	// Now here is a potential issue for stability,
	// if Re = 0, the fanning friction factor is not well behaved,
	// Hence it's better to use the laminar term at low Reynold's number
	//
	// we note that in the laminar regime, 
	// f = 16/Re
	// so f*Re^2 = 16*Re
	double transitionPoint = 1800.0;
	double fanningTerm;

	if (Re > transitionPoint)
	{
		fanningTerm = this.fanning(Re, this.roughnessRatio);
		fanningTerm *= Math.Pow(Re,2.0);
	}
	else
	{
		// otherwise we return 16/Re*Re^2 or 16*Re
		// or rather an interpolated version to preserve the
		// continuity of the points.
		IInterpolation _linear;

		IList<double> xValues = new List<double>();
		IList<double> yValues = new List<double>();
		xValues.Add(0.0);
		xValues.Add(transitionPoint);

		yValues.Add(0.0);
		yValues.Add(this.fanning(transitionPoint,this.roughnessRatio)*
				Math.Pow(transitionPoint,2.0));

		_linear = Interpolate.Linear(xValues,yValues);
		fanningTerm = _linear.Interpolate(Re);
	}






	//  BejanTerm
	//
	double bejanTerm;
	bejanTerm = 32.0 * this.bejanNumber;
	bejanTerm *= Math.Pow(4.0*this.lengthToDiameter,-3);

	// to set this to zero, we need:
	//
	return fanningTerm - bejanTerm;

}
```

All unit tests still pass, which is a good thing!

```csharp

[Theory]
[InlineData(1800)]
[InlineData(1799)]
[InlineData(1801)]
[InlineData(0)]
public void continuityTest_dB_dRe(double Re){
	double roughnessRatio = 0.05;
	double lengthToDiameter = 10.0;

	// basically at Re=1800, i transit from
	// churchill correlation to 16/Re for dB_dRe
	// for the stabilised churchill
	// I just want to see how bad the discontinuity is
	//

	IFrictionFactorJacobian _churchill;
	IFrictionFactorJacobian _stabilisedChurchill;

	_churchill = new ChurchillFrictionFactorJacobian();
	_stabilisedChurchill = new StabilisedChurchillJacobian();

	double dB_dRe_reference;
	if(Re > 100){
		dB_dRe_reference = _churchill.
			dB_dRe(Re, roughnessRatio, lengthToDiameter);
	}
	else{
		dB_dRe_reference = 16 *Re;
	}

	//Act

	double dB_dRe_result = _stabilisedChurchill.
		dB_dRe(Re, roughnessRatio, lengthToDiameter);

	// Assert

	//Assert.Equal(dB_dRe_reference, dB_dRe_result,0);


	double errorMax = 0.002;
	// Act



	double error = Math.Abs(dB_dRe_result - dB_dRe_reference)/dB_dRe_reference;

	// Assert
	//

	// Assert.Equal(referenceDarcyFactor,resultDarcyFactor);
	if(Re == 0.0){
		Assert.Equal(dB_dRe_reference,
				dB_dRe_result);
		return;
	}
	Assert.True(error < errorMax);
	return;
}
```
So this was the test to test for the continuity issue. It adds a Re=0 test
for testing the dB_dRe at Re=0. So as to see if the undefined result comes in again.

Also it tests for the relative error in gradient between churchill and the 
linearly interpolated function. It seems to be less than 0.2% compared
to the normal churchill function. Meaning to say the gradient is rather smooth.
Even though there is a small kink, it is less than 0.2% change. 
And the function is absolutely continuous.





