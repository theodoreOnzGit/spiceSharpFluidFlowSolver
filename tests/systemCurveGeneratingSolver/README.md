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

First test of course, i need the subcircuit to work as per normal in a normal
test.


# issues

Now when i put pipes in parallel, i seem to get a reading of 0.0 mass flowrate
through them. Which is quite curious and frankly nonsensical. 

I'll probably deal with this later.

