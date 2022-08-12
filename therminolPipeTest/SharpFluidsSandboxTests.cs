using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using spiceSharpFluidFlowSolverLibraries;
using SharpFluids;


namespace therminolPipeTest;

public class SharpFluidsSandboxTests : testOutputHelper
{
	public SharpFluidsSandboxTests(ITestOutputHelper outputHelper):
		base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

    [Fact]
    public void sandboxForTherminolFluid()
    {
		// make a new therminol-VP1 fluid object

		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

		// set temperature and pressure

		Pressure atmosphericPressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature roomTemperature 
			= new EngineeringUnits.Temperature(293, TemperatureUnit.Kelvin);

		// update PT of therminol
		// updates the temperature and pressure of therminol

		therminol.UpdatePT(atmosphericPressure, roomTemperature);

		// obtain prandtl number
		//
		this.cout("prandtl number of therminol and room temp and pressure");
		this.cout(therminol.Prandtl.ToString());
    }
}
