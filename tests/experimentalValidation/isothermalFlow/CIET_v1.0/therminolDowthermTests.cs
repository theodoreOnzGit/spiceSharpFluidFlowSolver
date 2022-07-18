using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;
namespace tests;

public class therminolDowthermTests : testOutputHelper
{

	public therminolDowthermTests(ITestOutputHelper outputHelper):
		base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

    [Theory]
	[InlineData(1)]
    public void WhenFM40Component_FrictionFactorShouldEqualCorrelation(
			double pressureDrop)
    {

		FM40 flowmeter = new FM40("RNL3");
		flowmeter.Connect("0","in");



		var ckt2 = new Circuit(
				new VoltageSource("V1", "in", "0", pressureDrop),
				flowmeter
				);

		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(ckt2);

		// Assert
		Assert.Equal(0.0,
				steadyStateSim.simulationResult);
    }
}
