# systemCurveGeneratingSolver README


This is the test suite for systemCurveGeneratingSolver.

## premise

Basically when i tried joining isothermal pipes together in 
series, I would get solver instablity whenever i tried to 
use voltage sources or current sources.
Nevertheless, i am able to work on pipes in parallel. 
I've tested running a voltage source on three pipes in parallel.
And that doesn't have any issue at all converging.

In other words, when i pass a voltage (or pressureDrop) to a pipe, 
i can immediately get a current or mass flowrate.

When i repeat this several times, i am able to get the system curve
of the pipe. A system curve states that for a set flowrate, expect
this much pressure drop.

If i can generate system curves for several pipes at a time, i can
basically stack them one on top of another. 

## some math

suppose i have a system curve for a pipe

$$pressureDrop = f (massFlowRate) $$

Now if i have five pipes in series, i can use:


$$pressureDrop_{total} = f_{pipe1} (massFlowRate) 
+f_{pipe2} (massFlowRate) 
+f_{pipe3} (massFlowRate) 
+f_{pipe4} (massFlowRate) 
+f_{pipe5} (massFlowRate) 
$$

The same can be applied in genearl for components in series.

For components in parallel, suppose we have five components.

i would sum the mass flowrates up instead of pressureDrops.

Suppose i could get a correlation as follows

$$massFlowRate = g (pressureDrop) $$

$$massFlowrate_{total} = g_{pipe1} (pressureDrop) 
+g_{pipe2} (pressureDrop) 
+g_{pipe3} (pressureDrop) 
+g_{pipe4} (pressureDrop) 
+g_{pipe5} (pressureDrop) 
$$

With that we can essentially sum the operating curves together to get pressure profiles at
various points of the system.

## obtaining pressure profiles

Suppose i wanted the pressure profile across a series of pipes.

I would either specify a total pressure drop or a specified mass flowrate.

The total pressure drop could be converted into a mass flowrate by
iteration. After that, knowing the system curves of each pipe,
I would feed this mass flowrate into the pipe. And there we go.


## obtaining mass flowrate from pressure drop across system

The first step would be to generate a system curve by summing up all the
system curves as before. 

We would then set a numerical iterator to use some bisection algorithm
so that mass flowrate is steadily adjusted until the total pressure drop
converges on the fixed pressure drop.

Once that is found, we have the mass flowrate.

## obtaining system level curves

We could place all the pipes in parallel. And set a fixed pressure across
all of them using a common voltage source.

Then observe the mass flowrate across each branch. Log that as one data point.
Repeat it for all the desired pressure drops.

We should have lots of data once we do this. And them sum the curves as 
described before.  

### FluidEntity and FluidEntityCollections

The way i chose to sum the curves is to create a fluidEntityCollections.

Normally components in Spicesharp inherit from entity and implement
IEntity. These will be put into a Circuit which is actually an
entityCollection. EntityCollections implement IEnumerable<IEntity>
and ICollection<IEntity>.

Collections and Enumerables are good because i can use a foreach
loop to repeat a summation operation. Synchronously or asynchronously.
Hopefully that i may run things in parallel. 

However to get pressure drop from each entity and sum them up,
i need an interface to enable this summation process. Such that
each entity is able to give me a pressure drop for a given flowrate. 
Mass flowrate to be precise. 

This applies only to pipes in series. For parallel pipes, another method
is to be used. 

For this i created new fluidEntity classes and interfaces.

The IFluidEntity interface will contain getPressureDrop and 
getKinematicPressureDrop methods to get the pressureDrop of the 
components. It will inherit from IEntity because i want extend 
the entities rather than make a whole new class.

FluidEntities will inherit from Entities in a similar manner and
only add the base classes. However, we may or may not use the
fluidEntities at the end of the day, since we can just make our
components implement the IFluidEntity interface.

These objects implementing IFluidEntity should be placed into
and EntityCollection. EntityCollections use Dictionaries under
the hood to store entities and implement the IEnumerable<IEntity>
and ICollection<IEntity> interface. 

However, making a FluidEntityCollection which implements both
IEnumerable<IEntity> and IEnumerable<IFluidEntity> is truoblesome.

Hence, what i do is to just get FluidEntityCollection to store a
separate IFluidEntity dictionary. Whenever i add entites to the 
FluidEntityCollection, I will have a method to check whether
it implements IFluidEntity, and if it does, I will add the entry
to the FluidEntityDictionary. Otherwise, don't add.

The same logic is used for removal. If the IEntity object added
also happens to fulfil IFluidEntity, then remove the entry from
the dictionary, or else do nothing. 

With this in place, I have a structure to help me implement 
IFluidEntityCollection. It will have two methods currently,
one to getPressureDrop, and one to getKinematicPressureDrop,
and it will force the implementation of a property:
FluidEntityDictionary. 

With this is place, we can work now on the FluidSeriesCircuit.

We expect it to behave like a normal circuit, but when IFluidEntities
are added to it, we want it to be able to calculate the sum of 
pressureDrops across all of them. 

Therefore when testing, we should get a FluidSeriesCircuit to perform
all the standard pipeTests and we should expect no issue.

But when pressureDrops are expected, i should expect a nonzero value.

### FluidSeriesCircuit

So far, i have done up a simple test of putting fluid Components in series,
and thus, i will just sum up all the pressure drops within. 

```csharp

[Theory]
[InlineData(1.45)]
[InlineData(-1.45)]
[InlineData(-1e-2)]
[InlineData(1e-2)]
[InlineData(0.0)]
public void When_FluidSeriesCircuitPressureDropExpect3xPressureDrop(
		double kinematicPressureDropVal){

	// Setup the simulation and export our current
	Component preCastPipe = new IsothermalPipe("isothermalPipe1");
	IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
	testPipe.Connect("pumpOutlet","1");
	preCastPipe = new IsothermalPipe("isothermalPipe2");
	IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
	testPipe2.Connect("1","2");
	preCastPipe = new IsothermalPipe("isothermalPipe3");
	IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
	testPipe3.Connect("2","0");

	// Build the circuit
	SpiceSharp.Entities.IFluidEntityCollection ckt = new FluidSeriesCircuit(
			new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
			testPipe,
			testPipe2,
			testPipe3
			);

	// first we get the Bejan number
	// for a pipe which is 3x as long
	//
	// so this means that my pipeLength is 3x as long
	// Be_L is about 3x as long in other words
	//
	MassFlow returnMassFlowRateValue(double kinematicPressureDropValJoulePerKg){

		double Be;
		Be = kinematicPressureDropValJoulePerKg;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter)*3.0;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter)*3.0;
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);

		double Re;
		ChurchillFrictionFactorJacobian _jacobianObject;
		_jacobianObject = new ChurchillFrictionFactorJacobian();
		double roughnessRatio = testPipe.Parameters.roughnessRatio();
		double lengthToDiameter = testPipe.Parameters.lengthToDiameter();
		// now for 3 pipes in series, my length is actually 3 times as long
		// so i need to multiply my L/D ratio by 3
		lengthToDiameter *= 3.0;
		Re = _jacobianObject.getRe(Be,roughnessRatio,lengthToDiameter);

		MassFlow massFlowRate;
		massFlowRate = testPipe.Parameters.fluidViscosity*
			testPipe.Parameters.crossSectionalArea()/
			testPipe.Parameters.hydraulicDiameter*
			Re;

		return massFlowRate.ToUnit(MassFlowUnit.KilogramPerSecond);
	}

	MassFlow massFlowRate =
		returnMassFlowRateValue(kinematicPressureDropVal);

	// Act
	// now if i feed in this massFlowrate, i should get
	// the pressureDrop as before

	SpecificEnergy kinematicPressureDropResult;
	kinematicPressureDropResult = ckt.getKinematicPressureDrop(
			massFlowRate);

	double kinematicPressureDropResultVal
		= kinematicPressureDropResult.As(
				SpecificEnergyUnit.JoulePerKilogram);

	// Assert
	Assert.Equal(kinematicPressureDropVal,
			kinematicPressureDropResultVal,3);

}
```

Basically the test here is to see if using one pipe 3x as long will produce
the same pressureDrop as 3 pipes in series. Instead of using spiceSharp's solvers
however, i will use the IFluidEntity Solvers.

The tests were stable and performed satisfactorily in the turbulent region.

### FluidParallelSubCircuit and FluidSeriesSubCircuit

Now FluidSeriesCircuit would be sufficient IF, we only had pipes in series.

Furthermore, we are not really using spice solvers. We are using in house solvers
to produce a solution. A tad too good to be an initial guess but it's ok. 

But now we have parallel pipes as well. We shall, for now constrain our
solvers to forced flows. Natural convection flows can come later.

Suppose i have two branches of two different pipes in parallel, how then would
i solve it?

SpiceSharp already has a structure in place called the SubCircuit which can
represent parts of a circuit. 

I can then create a FluidParallelSubCircuit class which has in it IFluidEntities
or IFluidEntityCollections. Each of these IFluidEntities will represent one
branch. 

How then can we deal with the branches themselves? I could create a 
FluidSeriesSubcircuit class. And within it, it will be a collection of 
IFluidEntities in series. These will then be put into the 
FluidParallelSubCircuit classes and then the FluidParallelSubCircuit will
be put into the FluidSeriesCircuit.

Each of these subcircuit classes would inherit from SubCircuit classes 
in spicesharp, but again, will implment the IFluidEntityCollection interface.

To begin, i can put a fluidParallelSubCircuit into the FluidSeriesCircuit
as its only component. This is perhaps the simplest test since spiceSharp has
no problem dealing with components in parallel configuration. 

The first thing of course is to use a voltage source or fixed pressureDrop source.
I want to check if putting 3 pipes in parallel using spiceSharp's default solvers
would yield the same mass flowrate values as the FluidParallelSubCircuit solvers.


now i've gotten them to work in tandem no problem, but they take excessively long
to solve a simple circuit...


```zsh
  Passed tests.fluidEntityTests.When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_
expectCorrectFlow(pressureDrop: 0) [4 s]
  Passed tests.fluidEntityTests.When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_
expectCorrectFlow(pressureDrop: 1.45) [22 s]
  Passed tests.fluidEntityTests.When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_
expectCorrectFlow(pressureDrop: 0.45) [59 s]
Test Run Successful.
Total tests: 274
     Passed: 245
    Skipped: 29
 Total time: 1.8588 Minutes
```
This is quite TOO LONG for a digitial twin. The algorithm which has nested iterative loops
is just not suitable for the speed. There must be a way of speeding things up. 

Which means that either we use this method to generate a systemCurve of approximate values,
or we use asynchronous or parallel methods to really speed up calculations. 

### solutions to help speed up calculations.

Mostly, the issue here has to deal with iterative calculations. That means
for pipes in general, to get my mass flowrate given a pressure drop,

one has to use MathNet Numerics methods to obtain pressuredrop by guessing 
flowrate, closer and closer until convergence is reached.

The other way to do it is to use a systemcurve sort of method, which means
i obtain a nondimensional graph of nondimensionalised pressure drop (Be_D)
against Re for the given L/D ratio and obtain an interpolated graph.

One question is what is the appropriate Re spacing for interpolation?

For linear interpolation, the interpolation spacing is linear for linear
systems.

If y=5x, then one could use two points and interpolate between them
as much as you wanted.

This is the case for laminar region where Bejan number and Reynold's number
are linearly related.

For $y=5x^2$, then we have a different issue.

We could also use linear interpolation, however, instead of interpolating between
x and y, we can interpolate between $x^2$ and y. Thus we only need two points.

It saves us a lot of RAM if we don't have to do linear interpolation over several
points.

This is good for the fully turbulent region or where K, the loss term, is the 
biggest. 

Here is where we have a $Be = k Re^2$ correlation.

This template is good for fLDK type components. 

$$Be_D = 0.5 Re^2 (f \frac{L}{D} + K)$$

However, for interpolations to be quick, one has to have a single dataset of
Be vs Re. Or a function. 

I need to have a function of Re explicit in Be.

I think three different interpolation schemes can be used. 

For linear regions, if we interpolate Re against Be, only two points need be
used. This is for pipes.

Yet in the space of all Reynold's numbers, it seems that a logarithm type 
interpolation would be more fitting.

But let's start simple and see if it suffices. Just apply a linear interpolation
and sample maybe at intervals of Re=100.

#### interpolation strategies

The most straightforward interpolation strategy is linear interpolation.

This works great for any region provided one has enough datapoints. 

Nevertheless, that's also where things can become challenging, because in
nonlinear regions, we may need many many many points to interpolate properly.

I tried out linear interpolation and it seemed to work well.

I suppose this linear interpolation class should be abstracted to its own
class since it will be used a lot regardless of component type.

Also how many data points do we want to have? 1000? 10000? I'll cap it at
1000 for now. Because for CFD, 1 million points usually needs about 1-3GB
of RAM to solve. And i guess for this, 1000 points would need 1-3MB.

Multiply that by 100 components, we may need around 100-300 MB of RAM. If
we have 10,000 points, then 1-3GB of RAM may be needed. Depending on the system,
it may freeze up the computer. 

So an upper interpolation limit of maybe 1000-2000 points will be used.
And better to have an IDisposable interface to make sure that things get disposed
of to clear memory. 

If we are stuck at 1000 points,

then linear interpolation won't necessarily do for high Re.

For example, for Re at 1e12, you need 1e10 interpolation points. Not ideal.

Instead we can note that towards highly turbulent region, the resistance
is like 

$$Be = K * Re^2$$

Because at that point, the friction factor is kind of constant. If a quadratic
correlation is used, we can pretty much use a polynomial to interpolate this.

In other words, use spline interpolation (piecewise polyonomial), 
but have the points logarithmically spaced. Perhaps about 30-50 points for each
order of magnitude. This is because in the transition region we want to pay
attention to the curves and all.

Given 12 orders of magnitude from 1e0 to 1e12, we would need about 600 points
in total.

The first point of course being (0,0).

However, we can perhaps take log10 of the number, and split evenly.
So log10(1e12) = 12, and split evenly we have about 12/600 = 0.02.

so the next point is

$$Re = 10^(0.02) $$
Then we sample Be there.

The following point is

$$Re = 10^(0.04)$$

The generalised nth point then is:

$$Re = 10^(0.02n)$$

This linear interpolation method has already borne fruit:

```zsh

  Passed tests.fluidEntityTests.When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_
expectCorrectFlow(pressureDrop: 1.45) [16 s]

  Failed tests.fluidEntityTests.When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_
expectCorrectFlow(pressureDrop: 0.45) [56 s]
  Error Message:
   Assert.Equal() Failure
Expected: 6063.245 (rounded from 6063.2452983508965)
Actual:   6063.246 (rounded from 6063.24624547323)
```
This is quite a significant shave off of time, from 22s before to 16s now,
and for the 0.45 pressureDrop tests, we have a shave off from 59s to 53s.

Obviously still some improvement, but shaving any amount of time off is a big boon
in itself.

Now this may not be as precise as the methods before. However, the interpolation
is so close and can cover the entire Re range without going kaput. I think
it's a reasonable tradeoff.

Both entities here, seem to give the correct values. Now I want to do the same
for each parallel entity so that it doesn't require the same level of 
iteration.




# issues

## can't read mass flowrate from resistor or pipe

Now when i put pipes in parallel, i seem to get a reading of 0.0 mass flowrate
through them. Which is quite curious and frankly nonsensical. 

I'll probably deal with this later.

## possible race conditions

When i unit tested this code, i often came across conditions like so:

```zsh

  Failed tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(pressureDrop: 0) [117
 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[gm²/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(Double pressureDrop) in 
/home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/pipesAndValves/pipesAndValv
esUnitTest.cs:line 411
```

Here's one on 1206 pm 27 jul 2022:

```zsh
  Failed tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(pressureDrop: 0) [193
 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [J/kg] as expected! Your Unit is a 
[gm/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.SpecificEnergy.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dDeltaP_dRe(Double Re, Double roughnessRatio, Double len
gthToDiameter, Length lengthScale, KinematicViscosity nu) in /home/teddy0/Documents/youTub
e/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFri
ctionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 141
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, Double Re, Double roughnessRatio, Length pipeLength, Ki
nematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowS
olver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/S
tabilisedChurchillJacobian.cs:line 221
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Double roughnessRatio, Len
gth pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/sp
iceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictio
nFactor/Jacobian/StabilisedChurchillJacobian.cs:line 291
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Length absoluteRoughness, 
Length pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube
/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFric
tionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 348
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 15
7
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(Double pressureDrop) in 
/home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/pipesAndValves/pipesAndValv
esUnitTest.cs:line 425
  Failed tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(pressureDrop: 0)
 [193 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [J/kg] as expected! Your Unit is a 
[gm/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.SpecificEnergy.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dDeltaP_dRe(Double Re, Double roughnessRatio, Double len
gthToDiameter, Length lengthScale, KinematicViscosity nu) in /home/teddy0/Documents/youTub
e/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFri
ctionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 141
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, Double Re, Double roughnessRatio, Length pipeLength, Ki
nematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowS
olver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/S
tabilisedChurchillJacobian.cs:line 221
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Double roughnessRatio, Len
gth pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/sp
iceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictio
nFactor/Jacobian/StabilisedChurchillJacobian.cs:line 291
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Length absoluteRoughness, 
Length pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube
/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFric
tionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 348
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 15
7
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(Double pressureDrop
) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/systemCurveGeneratingS
olver/systemCurveUnitTest.cs:line 272
```

Another one at 1:37pm 27 Jul 2022

```zsh

  Failed tests.fluidEntityTests.WhenDynamicPressureDropSuppliedExpectMassFlowrateValue(kin
ematicPressureDropVal: 0) [74 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [J/kg] as expected! Your Unit is a 
[gm/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.SpecificEnergy.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dDeltaP_dRe(Double Re, Double roughnessRatio, Double len
gthToDiameter, Length lengthScale, KinematicViscosity nu) in /home/teddy0/Documents/youTub
e/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFri
ctionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 141
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, Double Re, Double roughnessRatio, Length pipeLength, Ki
nematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowS
olver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/S
tabilisedChurchillJacobian.cs:line 221
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Double roughnessRatio, Len
gth pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/sp
iceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictio
nFactor/Jacobian/StabilisedChurchillJacobian.cs:line 291
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Length absoluteRoughness, 
Length pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube
/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFric
tionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 348
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 15
7
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.fluidEntityTests.WhenDynamicPressureDropSuppliedExpectMassFlowrateValue(Double
 kinematicPressureDropVal) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tes
ts/systemCurveGeneratingSolver/FluidEntityTests.cs:line 382
```

Another race condition 3:19pm 

```zsh

  Failed tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(pressureDrop: 0)
 [170 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[m]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dmdRe(Area crossSectionalArea, DynamicViscosity fluidVis
cosity, Length hydraulicDiameter) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSol
ver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/Sta
bilisedChurchillJacobian.cs:line 159
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 13
9
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(Double pressureDrop
) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/systemCurveGeneratingS
olver/systemCurveUnitTest.cs:line 272
```

Another race condition at 3:28pm
```zsh

  Failed tests.therminolDowthermTests.WhenFM40InSeries3x_Expect3xPressureDropSlowFlow(pres
sureDrop: 1) [184 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[m]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at flowmeterFM40Jacobian.dmdRe(Area crossSectionalArea, DynamicViscosity fluidViscosity
, Length hydraulicDiameter) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/sp
iceSharpFluidFlowSolverLibraries/pipesAndValves/Math/MixedComponentsFrictionFactor/Jacobia
n/flowmeterFM40Jacobian.cs:line 62
   at SpiceSharp.Components.FM40Behaviors.BiasingBehavior.SpiceSharp.Behaviors.IBiasingBeh
avior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFl
owSolverLibraries/pipesAndValves/ValvesAndComponents/FM40/BiasingBehavior.cs:line 113
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.IterateSourceStepping(Int32 maxIterations, 
Int32 steps)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 45
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.therminolDowthermTests.WhenFM40InSeries3x_Expect3xPressureDropSlowFlow(Double 
pressureDrop) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/experiment
alValidation/isothermalFlow/CIET_v1.0/therminolDowthermTests.cs:line 197
```

Another race condition 5:10pm

```zsh

  Failed tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(pressureDrop: 0)
 [167 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [J/kg] as expected! Your Unit is a 
[gm/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.SpecificEnergy.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dDeltaP_dRe(Double Re, Double roughnessRatio, Double len
gthToDiameter, Length lengthScale, KinematicViscosity nu) in /home/teddy0/Documents/youTub
e/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFri
ctionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 141
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, Double Re, Double roughnessRatio, Length pipeLength, Ki
nematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowS
olver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/S
tabilisedChurchillJacobian.cs:line 221
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Double roughnessRatio, Len
gth pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/sp
iceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictio
nFactor/Jacobian/StabilisedChurchillJacobian.cs:line 291
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Length absoluteRoughness, 
Length pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube
/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFric
tionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 348
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 15
7
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.systemCurveGeneratingSolver.When_parallelSetupExpect3xFlow(Double pressureDrop
) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/systemCurveGeneratingS
olver/systemCurveUnitTest.cs:line 272
```

These bugs were non repeatable and when i restarted the tests, they would often
disappear. I suspect this may be some form of race condition causing the bug.


One observation for these errors so far: they all seem to come from the biasing 
SImulation run especially when i try to get my stabilised Jacobian dDeltaP_dRe 
and dm_dRe.

Here is the attempted solution (tried on 27 jul 2022 5:25pm):
previously my code was
```zsh
	public IFrictionFactorJacobian jacobianObject = 
	new StabilisedChurchillJacobian();
```

Which basically means one instance of this jacobian object would be instantiated
per baseparameter object. And this was subsequently used once per instance
in BiasingBehavior. 

So if multiple instances of BiasingBehavior were run, then we would be using this
same shared jacobian object for all the solving. This is inviting race conditions
to come in.

I'd rather instantiate new instances of jacobian object when this method is called
so that i don't shared the object.

I may hog ram and slow the program down slightly, but that's better than a 
race condition.

```csharp
public IFrictionFactorJacobian jacobianObject(){
	return new StabilisedChurchillJacobian();
}
```

Same thing goes for BiasingBehavior, when the jacobian object is accessed,
i want this to create new instances of the ChurchillJacobianObjects.

```csharp

private IFrictionFactorJacobian _jacobianObject(){
	return this._bp.jacobianObject();
```

Only when the Load() method is used, then i'll store the object 
to save memory.

```csharp

IFrictionFactorJacobian _jacobianObject =
this._jacobianObject();

bejanNumber = _jacobianObject.getBejanNumber(
		pressureDrop,
		fluidKinViscosity,
		pipeLength);
```

If this is not effective at preventing race conditions, i'll change all the
jacobian object method accessing steps like so:
```zsh
bejanNumber = _jacobianObject().getBejanNumber(
		pressureDrop,
		fluidKinViscosity,
		pipeLength);
```


12:32pm 28 jul 2022, have another race condition:

```zsh

  Failed tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(pressureDrop: 0) [152
 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[gm²/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 14
8
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(Double pressureDrop) in 
/home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/pipesAndValves/pipesAndValv
esUnitTest.cs:line 425
  Failed tests.therminolDowthermTests.WhenFM40InSeries3x_Expect3xPressureDropSlowFlow(pres
```

To prevent this, i removed the intermediate unit in the dmdRe code:

```csharp

public MassFlow dmdRe(Area crossSectionalArea,
	DynamicViscosity fluidViscosity,
	Length hydraulicDiameter){

crossSectionalArea = crossSectionalArea.ToUnit(AreaUnit.SquareMeter);
fluidViscosity = fluidViscosity.ToUnit(DynamicViscosityUnit.PascalSecond);
hydraulicDiameter = hydraulicDiameter.ToUnit(LengthUnit.Meter);

var intermediateUnitResult = crossSectionalArea
*fluidViscosity
/hydraulicDiameter;

MassFlow derivativeResult;
derivativeResult = (MassFlow)intermediateUnitResult;

return derivativeResult;

}
public SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
		double lengthToDiameter,
		Length lengthScale,
		KinematicViscosity nu){

	lengthScale = lengthScale.ToUnit(LengthUnit.Meter);
	nu = nu.ToUnit(KinematicViscosityUnit.SquareMeterPerSecond);
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

It appears this unit casting thing is possibly not thread safe, i got rid of all the unit casting:

```csharp

public MassFlow dmdRe(Area crossSectionalArea,
		DynamicViscosity fluidViscosity,
		Length hydraulicDiameter){

	crossSectionalArea = crossSectionalArea.ToUnit(
			AreaUnit.SquareMeter);

	fluidViscosity = fluidViscosity.ToUnit(
			DynamicViscosityUnit.PascalSecond);

	hydraulicDiameter = hydraulicDiameter.ToUnit(
			LengthUnit.Meter);

	MassFlow derivativeResult = crossSectionalArea
		*fluidViscosity
		/hydraulicDiameter;

	derivativeResult = derivativeResult.ToUnit(
			MassFlowUnit.KilogramPerSecond);

	return derivativeResult;

}

public SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
		double lengthToDiameter,
		Length lengthScale,
		KinematicViscosity nu){

	lengthScale = lengthScale.ToUnit(LengthUnit.Meter);
	nu = nu.ToUnit(KinematicViscosityUnit.SquareMeterPerSecond);
	// dDeltaP_dRe will be in specific energy
	// SI unit is: m^2/s^2 
	// this is the same unit as kinematic pressure
	SpecificEnergy derivativeResult = nu.Pow(2)
		/lengthScale.Pow(2)
		*this.dB_dRe(Re,roughnessRatio, lengthToDiameter);

	return derivativeResult;
}
```

This way the code is neater, and we won't have implicit conversions 
or anything of this sort. I'll keep looking for race condition errors...

3:02pm 28 jul 2022, another race condition

```zsh

  Failed tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(pressureDrop: 0) [105
 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[gm²/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(Double pressureDrop) in 
/home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/pipesAndValves/pipesAndValv
esUnitTest.cs:line 411

```


3:13 pm 28 jul 2022

```zsh

  Failed tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(pressureDrop: 0) [101
 ms]
  Error Message:
   EngineeringUnits.WrongUnitException : This is NOT a [kg/s] as expected! Your Unit is a 
[m²]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.MassFlow.op_Implicit(UnknownUnit Unit)
   at tests.pipesAndValvesUnitTest.When_parallelSetupExpect3xFlow(Double pressureDrop) in 
/home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/pipesAndValves/pipesAndValv
esUnitTest.cs:line 405
```


```zsh

  Failed tests.therminolDowthermTests.WhenFM40InSeries3x_Expect3xPressureDropSlowFlow(pres
sureDrop: 1) [181 ms]
  Error Message:
   System.DivideByZeroException : 1 shall be divided by zero.
  Stack Trace:
     at Fractions.Fraction.Divide(Fraction divisor)
   at Fractions.Fraction.op_Division(Fraction a, Fraction b)
   at EngineeringUnits.UnitSystem.ConvertionFactor(UnitSystem To)
   at EngineeringUnits.BaseUnit.GetValueAs(UnitSystem To)
   at EngineeringUnits.BaseUnit.GetValueAsDouble(UnitSystem To)
   at EngineeringUnits.Angle.As(AngleUnit ReturnInThisUnit)
   at SpiceSharp.Components.FM40Behaviors.BiasingBehavior.SpiceSharp.Behaviors.IBiasingBeh
avior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFl
owSolverLibraries/pipesAndValves/ValvesAndComponents/FM40/BiasingBehavior.cs:line 73
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.IterateSourceStepping(Int32 maxIterations, 
Int32 steps)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 45
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.therminolDowthermTests.WhenFM40InSeries3x_Expect3xPressureDropSlowFlow(Double 
pressureDrop) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tests/experiment
alValidation/isothermalFlow/CIET_v1.0/therminolDowthermTests.cs:line 197
```


another one 28 jul 2022 3:40pm

```zsh

   EngineeringUnits.WrongUnitException : This is NOT a [J/kg] as expected! Your Unit is a 
[gm²/cms]
  Stack Trace:
     at EngineeringUnits.BaseUnit.UnitCheck(IUnitSystem a, IUnitSystem b)
   at EngineeringUnits.SpecificEnergy.op_Implicit(UnknownUnit Unit)
   at StabilisedChurchillJacobian.dDeltaP_dRe(Double Re, Double roughnessRatio, Double len
gthToDiameter, Length lengthScale, KinematicViscosity nu) in /home/teddy0/Documents/youTub
e/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFri
ctionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 133
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, Double Re, Double roughnessRatio, Length pipeLength, Ki
nematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowS
olver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictionFactor/Jacobian/S
tabilisedChurchillJacobian.cs:line 221
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Double roughnessRatio, Len
gth pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube/sp
iceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFrictio
nFactor/Jacobian/StabilisedChurchillJacobian.cs:line 291
   at StabilisedChurchillJacobian.dm_dPA(Area crossSectionalArea, DynamicViscosity fluidVi
scosity, Length hydraulicDiameter, SpecificEnergy pressureDrop, Length absoluteRoughness, 
Length pipeLength, KinematicViscosity fluidKinViscosity) in /home/teddy0/Documents/youTube
/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/pipesAndValves/Math/PipeFric
tionFactor/Jacobian/StabilisedChurchillJacobian.cs:line 348
   at SpiceSharp.Components.IsothermalPipeBehaviors.BiasingBehavior.SpiceSharp.Behaviors.I
BiasingBehavior.Load() in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/spiceSh
arpFluidFlowSolverLibraries/pipesAndValves/Pipes/IsothermalPipe/BiasingBehavior.cs:line 16
2
   at SpiceSharp.Simulations.BiasingSimulation.Load()
   at SpiceSharp.Simulations.BiasingSimulation.Iterate(Int32 maxIterations)
   at SpiceSharp.Simulations.BiasingSimulation.Op(Int32 maxIterations)
   at SpiceSharp.Simulations.PrototypeSteadyStateFlowSimulation.Execute() in /home/teddy0/
Documents/youTube/spiceSharpFluidFlowSolver/spiceSharpFluidFlowSolverLibraries/simulations
/SteadyState/PrototypeSteadyStateFlowSimulation.cs:line 39
   at SpiceSharp.Simulations.Simulation.Run(IEntityCollection entities)
   at tests.fluidEntityTests.WhenDynamicPressureDropSuppliedExpectMassFlowrateValue(Double
 kinematicPressureDropVal) in /home/teddy0/Documents/youTube/spiceSharpFluidFlowSolver/tes
ts/systemCurveGeneratingSolver/FluidEntityTests.cs:line 382

```
