# FluidSubCircuits

These FluidComponents were meant to copy the SubCircuit definitions, but now
add methods to extend its capability, ie, to return pressureDrop for a given
mass flowrate. 

## implementation

### Getting a template

To create a FluidSubCircuit, i just copied the SubCircuit code and renamed the class
in order to give myself freedom to meddle with the class.

MockFluidSubCircuit is one such class where i change nothing except the 
class name, constructors and cloning methods from SubCircuit to 
MockFluidSubCircuit.

### Subcircuit definitions

FluidSubCircuits, both series and parallel, are meant to return pressureDrops
for a given mass flowrate. And they inherit from FluidEntity rather than Entity.

The entities for subcircuits are stored in the subcircuit definition. 
The definitions are stored in the SubCircuit Parameters class. 

Subcircuit definitions in turn take in IEntityCollections in their constructor. 


## ISubCircuitDefinition and SubCircuitDefinition

Unfortunately, ISubcircuitDefinition and SubCircuitDefinition do not use
FluidEntityCollections. They use EntityCollections by default.

Hence i will need to use FluidEntityCollections in IFluidSubcircuitDefinition.

However, when i try to change the Entities into FluidEntities, i find that
the code is so tightly integrated that i cannot change it freely without
wrecking other parts of the code.

Eg. when i change the Entities into IFluidEntityCollection, it will complain
that the clone method for Entities isn't implemented. 

It's almost no use to do this =/

What i can do for the FluidSubcircuit is to check if each entity is castable as
a IFluidEntity just like before, and then sum up their pressureDrops. 

Or for parallel case, currents. 

# FluidParallelSubCircuit

To handle fluid flow in parallel pipes, i use the FluidParallelSubCircuit class.

For such circuits, we can easily obtain mass flowrates given a pressure drop since
the solver will just apply the same pressure drop to each branch and obtain
the mass flowrate.

Getting it the other way round sometimes requires iteration however. And that slows
things down.

## interpolation methods

To help with this, we might try to map out a relationship of getting a 
nondimensionalised pressure drop $Be_D$ with respect to Re. This would be in 
a similar manner we want to get explicit relationship of getting a Re from
Be in a pipe scenario. 

We first map out Be vs Re by sampling points of Re and getting Be. And then
we use those points to construct an interpolation object, a graph if you will
in the programming sense. 

This would help us get a graph of:

$$2 Be_D = (f_{darcy} \frac{L}{D} + K) Re^2
$$

We assume that this nondimensionalised relationship remains the same for all 
values of Be and Re.

Similarly for pipe systems in parallel, a nondimensionalised relationship
could be thought up

$$Be_D = f(Re)$$

or 

$$Re = f^{-1} (Be_D) $$
$$Re = \frac{ flowrate}{ A_{XS}} \frac{D_H}{\mu}$$
$$Be_D = \frac{\Delta p D_H^2}{\nu^2} = \frac{\Delta P D_H^2}{\mu \nu}$$

the question is, how can we nondimensionalise it such that the relationship
remains constant regardless of Re and $Be_D$?

We have multiple lengthscales and velocities to choose from. How can we pick
one that represents our case?

We normally have massflowrates and pipe cross sectional areas to help us
compute reynold's numbers.

It is quite intuitive that mass flowrates should be added up, and likewise
cross sectional areas can be added up as well.

The question is that for hydraulic diameter $D_H$. 

Thing is i already developed an method before to sample Be from Re in pipes.

I noticed that at high reynolds number, the pressure drop goes as the square
of mass flowrate. Or in terms of nondimensionalised variables, Be goes as 
$Re^2$ for high Re. So at high Re, a polynomial interpolation method should
suffice even for two points.

Less points should be used for high Re, and more points should be used for
interpolating the transition region.

That led me to think of using cubic spline interpolation for the entire Re range
and logarithmically spaced sampling points. So that at high Re, less points
are used, so as to save on RAM and calculation time, and at low Re and transition
Re, more sampling points are used.

If i were to constrain myself to this same method, then i would need to find
a way that the transition from laminar regime, where Be and Re relate linearly
or non quadratically, would happen at 2300 < Re < 4000. This way, i can ensure
that the transition region has sufficient points to help interpolate Be from
Re in that region, or vice versa.


For this we shall have a thought experiment:

Suppose we have two identical pipes in parallel,

using the above definitions, and if we were to take the overall hydraulic
diameter as the sum of the components, we would get the following:

$$Re = \frac{2 flowrate}{2 A_{XS}} \frac{2 D_H}{\mu}$$
$$Re = \frac{ flowrate}{ A_{XS}} \frac{2 D_H}{\mu}$$

Now for this case, the reynold's numbers for the parallel circuit would
be twice as high as the Reynold's number for the single pipe. Thus the
transition region would be from 4600 to 8000. This means either a new 
interpolation scheme should be used, or to keep the transition region
in a similar Re range, then we can define the hydraulic diameter as
the average hydraulic diameter of both pipes.

This is not good,

So we can instead take the hydraulic diameter to be the average of the hydraulic
diameter of both pipes. This way, we have:

$$Re = \frac{ flowrate}{ A_{XS}} \frac{ D_H}{\mu}$$

Thus, the transition region would more or less happen in a similar region.

Now suppose we have two pipes, one about 10x the diameter of the other:


The average hydraulic diameter is then 0.55D. Where D is the hydraulic
diameter of the larger pipe.

The cross sectional area is 1.01 $A_{XS}$ where $A_{XS}$ is the area of the
larger pipe. We can now see most of the flow goes through the pipe of higher
hydraulic diameter. This is by virtue of the larger pipe having so little flow
resistance. And i assume the pipe are of equal length.

Given that the smaller pipe adds a negligible amount of flowrate to the setup,
we can then see

$$Re \approx \frac{ flowrate}{ A_{XS}} \frac{0.55 D_H}{\mu}$$

Now in this case we see that the transition region might take place in 
Re about 1200-1300 to about 2200. This is just estimation.

Nevertheless, using logarithmically spaced points, the sampling interval in this
region is also twice as small as the region where Re is 2300 to 4000.

We can repeat this as many times as we'd like even up to Re = 120 due to the nature
of how we sample our logarithmically spaced points.

Now suppose we have another two pipes, one is about 1.5 times the diameter
of the other:

Again, the mass flowrates and cross sectional areas should scale more or less
equally provided the density and velocities are constant. They may not be..

But assume that isn't a problem yet (we can test this assumption later).

The average hydraulic diameter should be around 0.67 times of the larger pipe.

$$Re \approx \frac{ flowrate}{ A_{XS}} \frac{0.67 D_H}{\mu}$$

Again, the transition region here would happen below Re = 2300 due to the way
the Re scales.

This wouldn't be a problem either.

It is likely however, that more flowrate goes through the larger pipe, and we
can have potential situations where one pipe is in a laminar regime
and the other in a parallel regime. 

Here is where we consider the next case.


Suppose we have a pressure drop such that one pipe is laminar and the other
is turbulent. And unlike the above case, the flowrate through neither is 
negligible. Will this interpolation scheme then work?


For one pipe:

$$Be = k_1 Re$$

For the other:


$$Be = k_2 Re^2$$
$$Re = \sqrt{\frac{Be}{k_2}}$$

Now if we had a common pipe Re, we would be able to get Re from Be like so:

$$Re = \frac{Be}{k_1} + \sqrt{\frac{Be}{k_2}}$$

For this case, a polynomial spline interpolation method still seems feasible.


Now suppose we have a fLDK component with no clear transition region:

We have the following correlation

$$Be_D = k_1 Re + k_2 Re^2$$

For this, a cubic spline interpolation would also work regardless of the points
supplied, since this function is clearly quadratic.


It looks like the bases are reasonably checked and covered. I probably haven't 
turned over every stone, perhaps there are multiple pipes, or one pipe is in
a transition regime and the other is not, or one pipe is in a not fully turbulent
(smooth pipe) regime and the other is not.

Regardless, i think for a first iteration, it should do okay. There can be future
work done to adapt this for other systems. But for CIET, where flow is mostly
laminar, and we have pipes, this should be fine.

### estimation methods for Be

upon writing the code, i know i have a simple way for finding evenly spaced
Reynold's number points. However, i don't know a simple way for finding evenly
spaced Bejan number points... 

Bejan number is usually the dependent variable, not independent variable. So
there aren't usually Bejan number ranges i can think off the top of my head.

However, i know for pipes, there are bejan numbers corresponding to Reynold's
numbers. And i can assume the system behaves like a pipe in terms of distribution.

I will use the log spaced Reynold's numbers to guess the corresponding Bejan
numbers, and will use the Bejan numbers as guess points to get the corresponding
Reynold's numbers.

```csharp


for (int i = 0; i < 500; i++)
{
	// first i decide on a number of values to give
	double ReLogSpacing = 0.02;
	double ReGuessValue = Math.Pow(10,ReLogSpacing * i);

	// once we have a suitable Re, we need to get a Be
	// value,
	// so we convert Re into mass flowrate
	//
	// Now unfortunately, for the first round of iteration,
	// we cannot guess Re from Be non iteratively.
	// We have to guess Be first, and then from that get Re.
	//
	// Problem is i don't know what a typical Be range should be!
	// Nor where the transtion region should be for any typical
	// graph.
	//
	// Well one method we can try is this:
	//
	// we have a guess Reynold's number value, which we then
	// feed into a pipe equation like churchill
	// from that, we can guess a Bejan number to go with
	// And from this Bejan number, we can guess the actual
	// Reynold's number value

	IFrictionFactor _frictionFactorObj = 
		new ChurchillFrictionFactor();

	// so bejan number is:
	// Be = 0.5 *Re^2 (f L/D  + K)
	//
	// Now herein lines a problem,
	// if we were to  use this method to guess,
	// we need a proper L/D ratio too.
	// To keep it once more in a similar order of
	// magnitude, i would rather have L be the average
	// of both branch lengths
	//
	// I assume the cubic spline would take care of 
	// any variation due to K, what i'm watching out for
	// so i assume K = 0
	//
	// more importantly is modelling the transition region
	// or interpolating it with sufficient points
	// to get L/D ratio, we need the average branch lengths
	// and average hydraulic diameters

	double lengthToDiameter = this.getComponentLength().
		As(LengthUnit.Meter) / 
		this.getHydraulicDiameter().
		As(LengthUnit.Meter);

	// my roughness ratio here is guessed based on 
	// assuming cast iron pipes, just a guestimation
	// so not so important

	Length absoluteRoughness = new Length(
			0.15, LengthUnit.Millimeter);


	double roughnessRatio = absoluteRoughness.As(LengthUnit.Meter)/ 
		this.getHydraulicDiameter().As(LengthUnit.Meter);


	double darcyFrictionFactor = _frictionFactorObj.
		darcy(ReGuessValue, roughnessRatio);

	// i shall now shove these values in to obtain my Bejan number
	// Be_d = 0.5*Re(guess)^2 *f * L/D
	double bejanNumber = 0.5 * 
		Math.Pow(ReGuessValue,2.0) *
		lengthToDiameter *
		darcyFrictionFactor;
	// once we have this we can add the bejan number
	BeValues.Add(bejanNumber);

}
```

So this is how i will gues bejan number.

### Guessing Pressure Drop from Bejan number (parallel case)

Now we have a bejan number, we will need to apply a pressure drop across
this parallel array. So we will need an average kinematic viscosity so to speak
from this array of pipes and components.

If the pipes were isothermal, great, we only have one kinematic viscosity
with which to weight our Bejan numbers.

$$Be_D = \frac{\Delta p D^2}{\nu^2}$$

To understand how we got here, we should visit the underlying equations
This is for the pipe:

$$Be_D = 0.5 (\frac{L}{D} f_{darcy} + K} Re^2$$

$$Be_D = 0.5 (\frac{L}{D} f_{darcy} + K} Re^2$$

$$ \frac{\Delta p D^2}{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K} Re^2$$

$$ \frac{\Delta p D^2}{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{D_H^2}{\mu^2}$$

$$ \frac{\Delta p }{\nu^2} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{1}{\mu^2}$$

$$ \frac{\Delta p }{1} = 0.5 (\frac{L}{D} f_{darcy} + K} 
\frac{\dot{m}^2}{A_{xs}^2} \frac{1}{\rho^2}$$

For this case, we can see when it comes to viscosity, as long as the terms
can cancel out like this, it doesn't matter.

If the components were in series, we would simply sum up pressre drops across
all of them. However, we cannot since they are in parallel.

Instead, a single bejan number would give rise to multiple mass flowrates.

Now suppose we have two fLDK type components in parallel, which follow this
correlation:

$$Be_D = k_1 Re + k_2 Re^2$$
$$k_1 Re^2 + k_2  Re - Be_D = 0$$

If we were to use the quadratic formula

$$Re = \frac{1}{2k_1} (-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

Since we expect Re to be positive (mostly), or rather, take the same sign
as $Be_D$, that when $Be_D$ is positive, Re is positive, and vice versa,
we shall just assume both of them are positive.

$$Re = \frac{1}{2k_1} (-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

Now we see in this case, Re scales as the square root of (Be_D), at least
in terms of units, that is the case. For simplicity's sake, we shall use
this assumption to find a suitable average viscosity for parallel setup.


$$\frac{\dot{m}}{A_{xs}}  \frac{D_H}{\mu}
= \frac{1}{2k_1} (-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})$$


$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})$$

As a sanity check, we can note that when Be=0, $-k_2+ \sqrt{k_2^2} = 0$ and thus
the mass flowrate should be zero at zero pressure drop. This is the correct
expression.

Now this is for a single pipe so to speak. For two identical pipes, we can
express the total mass flow as:

$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 \pm \sqrt{k_2^2 + 4(k_1)(Be_D)})
$$

Assuming they are at different temperature due to the different viscosities,
we can assign $\mu_1$ and $\mu_2$ to describe the differing viscosities of each
pipe.

Note that this equation is simply a mass balance type equation across
the parallel branches. Ie, total mass flowrate across the parallel branches
is the sum of flow between each branch.


$$\dot{m} = \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(-k_2 + \sqrt{k_2^2 + 4(k_1)(Be_D)})
$$

in terms of units, so we can cancel the dimensionless terms out,

$$\dot{m} ~ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D})
+ \frac{A_{xs}\mu}{D_H}\frac{1}{2k_1} 
(\sqrt{Be_D})
$$

We already decided that for the area scaling, the sum of areas will be the
representative cross sectional area of the parallel setup, and the 
ensemble average hydraulic diameter will be the representative hydraulic diameter
of the mass flowrates. Then perhaps to keep Re in the same order of magnitude,
as individual pipe Re, i will use the ensemble average.



$$ \frac{A_{xsTotal} \mu_{avg}}{D_{Havg}} \sqrt{Be_avg}
~ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D}
+ \frac{A_{xs}\mu}{D_H} \sqrt{Be_D}
$$

Note here that as we bring out the kinematic viscosity terms $Be_D$


$$ \frac{A_{xsTotal} \mu_{avg}}{D_{Havg}} \sqrt{Be_avg}
~ \frac{A_{xs}\rho}{1} \sqrt{\Delta p}
+ \frac{A_{xs}\rho}{1} \sqrt{\Delta p}
$$

It seems that, if we just use simple ensemble average for kinematic
viscosity, that would work since most of the terms cancel out anyhow.

$$ \frac{A_{xsTotal} \rho_{avg}}{1} \sqrt{\Delta p}
~ \frac{A_{xs}\rho1}{1} \sqrt{\Delta p}
+ \frac{A_{xs}\rho2j}{1} \sqrt{\Delta p}
$$

Now for parallel setups, i believe the dynamic pressures across each branch
will be the same

$$ A_{xsTotal} \sqrt{\rho_{avg}} \sqrt{\Delta P}
~ A_{xs}\sqrt{\rho1} \sqrt{\Delta P}
+ A_{xs}\sqrt{\rho2} \sqrt{\Delta P}
$$

For this to hold, such that the Be and Re numbers are of a similar order of 
magnitude, we have this such equation for parallel setup:

$$ A_{xsTotal} \sqrt{\rho_{avg}}
= A_{xs1}\sqrt{\rho1} 
+ A_{xs2}\sqrt{\rho2} 
$$

$$  \sqrt{\rho_{avg}}
= \frac{1}{A_{xsTotal}} (A_{xs1}\sqrt{\rho1} 
+ A_{xs2}\sqrt{\rho2} )
$$

we have just found a way of weighting our average density. At least for
the more turbulent type regime. For laminar regimes, the square root
kind of disappears, so that we have a density weighted by areas.

Either way, we shouldn't be that far off.



Square the result and we should get our average density.

This will keep the average Re and hopefully Be as well to be in the same
order of magnitude.

With these scaling parameters, we should be able to get our pressure drops pretty
decently well. 


The area of the parallel setup is the sum of areas of each branch,
the mass flowrate is the sum  of mass flowrates of each branch,
hydraulic diameter is the ensemble average of hydraulic diameter of each branch,
kinematic viscosity is the ensemble average of kinematic viscosity of each branch,
finally, the density is weighted by area of each respective branch.

Therefore we need a way to return the areas, and densities of each pipe branch
or each fluid entity. So that the averaging can occur.

Also kinematic viscosity too.

I'll probably want to do a series of tests to confirm if the paralleSubCircuit
is obtaining the quantities as shown above. 

okay tests complete, here's one such example:


```csharp
[Theory]
[InlineData(0.5,0.1,0.3, 1.5,1.0,0.5)]
[InlineData(0.4,0.2,0.3, 0.9,0.88,1.3)]
public void WhenParallelSubCkt_getDensity_expectAreaWeightedSum(
		double diameter1,
		double diameter2,
		double diameter3,
		double kinVis1,
		double kinVis2,
		double kinVis3){


	// Setup
	//
	// First let's get the lengths and kinematic Viscosities
	//
	// Because my testpipe obtains density by finding ratios of 
	// kinematicViscosity to dynamic Viscosity
	// i will just randomly switch up the kinematic viscosity up

	Length hydraulicDiameter1 = new Length(diameter1, LengthUnit.Meter);
	Length hydraulicDiameter2 = new Length(diameter2, LengthUnit.Meter);
	Length hydraulicDiameter3 = new Length(diameter3, LengthUnit.Meter);

	KinematicViscosity kinViscosity1 = 
		new KinematicViscosity(kinVis1, 
				KinematicViscosityUnit.Centistokes);
	KinematicViscosity kinViscosity2 = 
		new KinematicViscosity(kinVis2, 
				KinematicViscosityUnit.Centistokes);
	KinematicViscosity kinViscosity3 = 
		new KinematicViscosity(kinVis3, 
				KinematicViscosityUnit.Centistokes);


	// next let's setup the pipes

	Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
	IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
	testPipe.Parameters.hydraulicDiameter = hydraulicDiameter1;
	testPipe.Parameters.fluidKinViscosity = kinViscosity1;
	testPipe.Connect("parallelIn","parallelOut");

	preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
	IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
	testPipe2.Parameters.hydraulicDiameter = hydraulicDiameter2;
	testPipe2.Parameters.fluidKinViscosity = kinViscosity2;
	testPipe2.Connect("parallelIn","parallelOut");

	preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
	IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
	testPipe3.Parameters.hydraulicDiameter = hydraulicDiameter3;
	testPipe3.Parameters.fluidKinViscosity = kinViscosity3;
	testPipe3.Connect("parallelIn","parallelOut");

	// let me get my total area from these pipes
	Area expectedTotalArea =
		(testPipe.getXSArea() + 
		 testPipe2.getXSArea() + 
		 testPipe3.getXSArea());

	// the above area will help me weight my densities
	// now let's get the expected density
	// were
	// sqrt(rho_avg) = 1/totalArea * (area1*sqrt(rho1) +
	// area2*sqrt(rho2) + area3*sqrt(rho3))

	Density _expectedAverageDensity;

	{
		double sqrtDensity;
		sqrtDensity = 
			testPipe.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe.getFluidDensity().As(DensityUnit.SI),0.5) +
			testPipe2.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe2.getFluidDensity().As(DensityUnit.SI),0.5) +
			testPipe3.getXSArea().As(AreaUnit.SI) *
			Math.Pow(testPipe3.getFluidDensity().As(DensityUnit.SI),0.5);

		sqrtDensity /= expectedTotalArea.As(AreaUnit.SI);
		double densityValue = 
			Math.Pow(sqrtDensity,2.0);

		_expectedAverageDensity = new Density(
				densityValue, DensityUnit.KilogramPerCubicMeter);
	}



	var subckt = new SubcircuitDefinition(new Circuit(
				testPipe,
				testPipe2,
				testPipe3),
			"parallelIn", "parallelOut");

	FluidParallelSubCircuit fluidSubCkt
		= new FluidParallelSubCircuit("X1", subckt);
	fluidSubCkt.Connect("out" , "0");

	//Act
	//
	Density _actualAverageDensity = 
		fluidSubCkt.getFluidDensity();


	// Assert

	Assert.Equal(_expectedAverageDensity.As(DensityUnit.SI),
			_actualAverageDensity.As(DensityUnit.SI),1
			);

}

```
Now for parallelSubCkts with this setup, what tests do we need?
Assume that each branch has been condensed into a single component.

0. basic functionality
a. if i use the nondimensionalised interpolation method across various Re
it should yield the same results as with a normal calculation (ie using the
bisection or whatever else implicit methods).
Meaning to say i'll want two classes of FluidParalleSubCircuit, one reference
class and one interpolated class. Otherwise, hook up a current source to two
IsothermalPipes in parallel. That will be the reference. Then test this against
the parallelSubCkts.

Unfortunately, i tried the current source to parallel setup, failed miserably.



1. Elevation tests, 
a. means if i elevate the branch i should expect the proper mass flowrate
given a pressure drop
b. it also means that if i elevate a branch, both branches should elevate
equally. Meaning to say both branches should have the same elevation change.
c. if i elevate branches and the temperatures change, i should expect a different
flowrate due to the temperature change (this is more of a density change however
probably do it later)

2. Density and viscosity change tests (more of temperature change)
a. if i calculate the mass flowrate from a pressure drop using the 
nondimensionalised correlation, and change density and viscosity halfway thru,
i should expect the massflowrate to be correct using the same nondimensionalised
correlation.


Also, i forgot a few unit tests for my IFluidEntity anyhow when using the 
interpolation thingy. Like the dynamic pressure tests are not done yet.
For Isothermal pipe it isn't implemented properly and for fluidparallel subckt
it's not even implemented.

## Nested tests with FluidSeriesCircuits

Now after the interpolation strategy, things seem to be working out for the 
most part. The code runs a lot faster. And the only significant time taken
to solve the FluidSeriesCircuit is the time taken to build the fluidSystemCurves.

Except when i try to use the getMassFlowRate within the FluidSeriesCircuits 
the iterations cause the solver to go beserk. And i get numerically unstable 
answers. Which to be fair, is kind of beyond interpolation range.

Now, my way of getting massFlowrate is to use an iterative root finding algorithm
which guesses pressureDrop by changing mass flowrate until the guessed 
pressureDrop matches that of the desired pressureDrop. My mass flowrates are tested
from -1e12 to 1e12 kg/s. Kind of too much tbh and it may be beyond the range of
interpolated values anyhow.


One possible way to change this is to change the range of roots of the mass flowrate
from maximum to minimum possible Reynold's number of 1e12 and -1e12 respectively.

Now to get a Reynold's number representative of the whole system, one must then
think of how to scale things properly. What is the hydraulic diameter, what is
the viscosity, what is the mass flowrate and what is the cross sectional area.

Can we use ensemble averages or must we use something more sophisticated?

I think we must develop some proper theory for FluidSeriesCircuits and
FluidSeriesSubCircuits on how to nondimensionalise things properly.

The methodology will be assume there is one representative component that produces
the same pressure drop for a given mass flowrate as a series of pipes and 
components. 






