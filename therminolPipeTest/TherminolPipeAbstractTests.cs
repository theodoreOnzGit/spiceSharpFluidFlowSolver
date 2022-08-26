using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using spiceSharpFluidFlowSolverLibraries;
using SharpFluids;


namespace therminolPipeTest;

public partial class TherminolComparisonTests : testOutputHelper
{


	[Fact]
	public void test1(){
		EngineeringUnits.Temperature temp = 
			new EngineeringUnits.Temperature(0.0,
					TemperatureUnit.SI);
		Assert.True(true);
	}

}
