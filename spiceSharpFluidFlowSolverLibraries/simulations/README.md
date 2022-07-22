# simulations

In this folder, you will find custom simulations which inherit
from Biasing Simulation class. 

This is because the standard operating point simulation
make it difficult to extract simulation data out of the class.

And other than printing data, it doesn't do all that much.

## MockSteadyStateFlowSimulation

This is nothing but a copy of the operating point class. Just a different
class name.

## ISteadyStateFlowSimulation

This is a public interface which inherits from IBiasingSimulation.

However it will include interfaces by which one can extract data from
the simulation.

in its first iteration

```csharp
public interface ISteadyStateFlowSimulation : 
IBiasingSimulation
{
	public double simulationResult { get; set; }
}
```

There's just an extra property which is present.


## PrototypeSteadyStateFlowSimulation

This contains the class for me to experiment with adding
new properties and methods to the original OP class.

In the first iteration:

```csharp
public class PrototypeSteadyStateFlowSimulation : 
	BiasingSimulation, ISteadyStateFlowSimulation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="OP"/> class.
	/// </summary>
	/// <param name="name">The name of the simulation.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
	public PrototypeSteadyStateFlowSimulation(string name)
		: base(name)
	{
	}

	double ISteadyStateFlowSimulation.
		simulationResult { get; set; }

	/// <inheritdoc/>
	protected override void Execute()
	{
		base.Execute();
		Op(BiasingParameters.DcMaxIterations);
		var exportargs = new ExportDataEventArgs(this);
		OnExport(exportargs);
	}
}
}
```

I'm just adding one simulationResult property with which to
extract simulation data.

However, I also got it to use several algorithms, eg. source stepping
as part of troubleshooting.

```csharp
protected override void Execute()
{
	// these two lings of code are for me to force source stepping
	//BiasingParameters.NoOperatingPointIterate = true;
	//BiasingParameters.GminSteps = 0;
	base.Execute();

	switch (simulationMode)
	{
		case "vanilla":
			Op(BiasingParameters.DcMaxIterations);
			break;

		case "sourceStepping":
			int maxiterations = 100;
			int sourceSteps = 10;
			IterateSourceStepping(maxiterations,sourceSteps);
			break;

		default:
			break;
	}
	var exportargs = new ExportDataEventArgs(this);
	OnExport(exportargs);
}
```

I gave a public property called simulationMode so that the 
PrototypeSteadyStateFlowSimulation could execute algorithms based
on user input

## SystemCurveSimulator

Now I faced a problem when connecting multiple nonlinear components
in series, be it churchill pipes or fLDK type components. 

Note that fLDK components here are such that


$$\left( f_{darcy} \frac{L}{D} + K \right) 
Re^2 = 2 Be_D$$

Where

$$Be_D = \frac{\Delta P D^2}{\rho \nu^2}$$

In laminar region, fLDK components do fine. But when it comes to 
turbulent regions, anything more than one component in series causes
numerical instabilities. 

I thought this was due to the transition region within the churchill
friction factor component. It was known that inflexion points caused
the Newton Raphson method to become [unstable](
http://home.zcu.cz/~tesarova/IP/Proceedings/Proc_2010/Files/030%20IP2010%20Veleba.pdf)
.


But that wasn't the case. 

However, it was known that changing initial conditions for newton
raphson could work. If a point is close enough to the actual solution,
then convergence is guaranteed.

The convergence regions of newton raphson methods have been mapped out
in literature using [fractals](
https://www.researchgate.net/profile/Zoltan-Kovacs-3/publication/277475242_Understanding_convergence_and_stability_of_the_Newton-Raphson_method/links/556b279f08aeab77722143e8/Understanding-convergence-and-stability-of-the-Newton-Raphson-method.pdf?origin=publication_detail)

What this means is that certain regions or initial guess points for
the newton raphson method produce good convergence properties,
other points produce a divide by zero sort of error.

To obtain a good initial guess, some thought must be put in.

### SystemCurve generation for initial guesses

One way to generate an initial guess very close if not at the solution
is to generate a system curve. For a set pressure drop going through
a series of pipes, the solver could set a constant mass flowrate
through each pipe, and each pipe component would produce
a pressure drop.

The mass flowrate versus pressure drop could be iterated for several
hundred points depending how much RAM is available.

This would then generate a curve of total pressure drop vs mass flowrate.
And it would have the data to generate a curve of the individual 
pipe pressure drops vs mass flowrate. 

Anytime a pressure drop is specified, the point with the nearest pressure
drop can be selected as an initial point. And the newton raphson solver
can do the rest. 

### System Curve generation as an individual solver

In fact, the system curve method can be a solver on its own. This means
that the total pressure drop can be a function of mass flowrate. 

And the summation of individial pipe pressure drops will amount to the
total pressure drop of the system. A single input single output 
iteration method can be solved via the Mathnet libraries as long as
bisection is used. This guarantees convergence, as long as there isn't
more than one root in the vicinity of the region. 

However, this means all the spice sharp work is not really useful 
anymore. 

The disadvantage of this method is that users will need to carefully
build an entire pipe network, specifying regions where series branches
are found, and where parallel branches are found. The algorthims used
to solve them will indeed be different. 


### Proof of Concept

Before getting ahead to develop full circuit solvers, it is useful
to demonstrate a proof of concept for this. 

The stages of development so far are:

1. How can we solve the pressure drop over a series of three pipes?
2. How can we solve the pressure drop and flowrate over a series
of three pipes and then a branched segment?


### Code details 

The first one is a simpler problem, there is conveniently an 
ICollection for us, or rather IEntityCollection.

The Circuit is the simplest form of IEntityCollection.

IEntityCollections inherit from IEnumerable<IEntity> and
ICollection<IEntity>.

The most important operation for this is to get pressure drops
for each component. Hopefully asynchronously to reduce computation time.

But we can deal with computation time later. 

Components inherit from Entity which fulfil the IEntity interface.

I'd like to make an IEntityCollection which is called fluidSeriesCircuit.

It will inherit from Circuit but is meant to do more. 

Here i want a method with which to get the total pressure 
drop across all components in series. 

Therefore i want a method called getTotalPressureDrop within
an interface IFluidEntityCollection.

Supposedly this will sum up all the pressure drops across all
fluid entities within this fluidSeriesCircuit. Therefore each
entity in the collection will need to have a method to obtain
its pressure drop given a mass flowrate. 

These will be called fluidEntities. Or more aptly fluidComponents.
FluidComponents will implement the IFluidEntity interface. Or better
yet, i'll just mirror the way spicesharp does it and create FluidEntity
classes which inherit from entity classes. 

IFluidEntity will inherit from IEntity. And have a method called
getPressureDrop. And return either dynamic or kinematic pressure drop
in engineering units. 

So ideally, i'd want a fluidSeriesCircuit to do this job of summing up
pressure drops, but still be able to perform the necessary spicesharp
calculations.























