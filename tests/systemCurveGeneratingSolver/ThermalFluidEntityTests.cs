using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public partial class ThermalFluidEntityTests : testOutputHelper
{
	public ThermalFluidEntityTests(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

	// when a thermal fluid entity (nonisothermal pipe) with 
	// heat transfer changes temperature
	// does the bejan graph still work?
	[Theory]
	[InlineData()]
	public void WhenThermalFluidEntityChangeTemperature_ExpectInterpolationCorrectResult(
			){
		throw new NotImplementedException();
	}

	// when a thermal fluid entity (nonisothermal pipe) with 
	// heat transfer changes temperature
	// does the bejan graph still work?
	// yes taking into account natural convection as well...
	[Theory]
	[InlineData()]
	public void WhenThermalFluidEntityCircuitNaturalConvection_ExpectCorrectResult(
			){
		throw new NotImplementedException();
	}

}
