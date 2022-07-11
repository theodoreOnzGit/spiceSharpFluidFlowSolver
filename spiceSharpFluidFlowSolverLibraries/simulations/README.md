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
