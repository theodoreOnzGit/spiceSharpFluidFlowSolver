using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using spiceSharpFluidFlowSolverLibraries;
using SharpFluids;


namespace therminolPipeTest;

public class TherminolPipeAbstractTests : testOutputHelper
{
	public TherminolPipeAbstractTests(ITestOutputHelper outputHelper):
		base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}


	[Fact]
	public void test1(){
		EngineeringUnits.Temperature temp = 
			new EngineeringUnits.Temperature(0.0,
					TemperatureUnit.SI);
		Assert.True(true);
	}

}
