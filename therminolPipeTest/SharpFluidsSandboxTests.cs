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

	[Theory]
	[InlineData(0.5)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	public void WhenTherminolChangePressureExpectPrandtlInvariance(
			double pressureBars){


		// Setup

		Pressure testPressure = Pressure.FromBar(pressureBars);
		
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

		// set temperature and pressure

		Pressure referencePressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature roomTemperature 
			= new EngineeringUnits.Temperature(293, TemperatureUnit.Kelvin);

		// update PT of therminol
		// updates the temperature and pressure of therminol

		therminol.UpdatePT(referencePressure, roomTemperature);

		// obtain prandtl number

		double referencePrandtlNumber = therminol.Prandtl;

		// Act
		therminol.UpdatePT(testPressure, roomTemperature);

		double testPrandtlNumber = therminol.Prandtl;

		// Assert
		//
		Assert.Equal(referencePrandtlNumber,testPrandtlNumber);
		
	}

	[Theory]
	[InlineData(20)]
	[InlineData(30)]
	[InlineData(40)]
	[InlineData(50)]
	[InlineData(60)]
	[InlineData(70)]
	[InlineData(80)]
	[InlineData(90)]
	[InlineData(100)]
	[InlineData(120)]
	[InlineData(130)]
	[InlineData(140)]
	[InlineData(150)]
	[InlineData(160)]
	[InlineData(170)]
	[InlineData(180)]
	public void WhenTemperatureVariedExpectTherminolAndDowthermPrandtlSame(
			double temperatureC){

		// Setup

		// set temperature and pressure for dowtherm and Therminol
		Pressure referencePressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature 
			= new EngineeringUnits.Temperature(temperatureC, 
					TemperatureUnit.DegreeCelsius);


		double referencePrandtlNumber;
		referencePrandtlNumber = this.getDowthermAViscosity(testTemperature) *
			this.getDowthermAConstantPressureHeatCapacity(testTemperature) / 
			this.getDowthermAThermalConductivity(testTemperature);
		

		// get therminol VP-1 fluid object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);




		// Act
		therminol.UpdatePT(referencePressure, testTemperature);

		double testPrandtlNumber = therminol.Prandtl;

		// Assert
		//
		Assert.Equal(referencePrandtlNumber,testPrandtlNumber,
				1);

	}



	public Density getDowthermADensity(EngineeringUnits.Temperature fluidTemp){

		this.rangeCheck(fluidTemp);
		double densityValueKgPerM3;
		densityValueKgPerM3 = 1078 - 0.85*fluidTemp.
			As(TemperatureUnit.DegreeCelsius);

		return new Density(densityValueKgPerM3,DensityUnit.KilogramPerCubicMeter);
	}

	public DynamicViscosity getDowthermAViscosity(
			EngineeringUnits.Temperature fluidTemp){

		this.rangeCheck(fluidTemp);
		double mu;
		mu = 0.130;
		mu /= Math.Pow(fluidTemp.As(TemperatureUnit.DegreeCelsius),
				1.072);

		return new DynamicViscosity(mu,
				DynamicViscosityUnit.PascalSecond);
	}

	public SpecificHeatCapacity getDowthermAConstantPressureHeatCapacity(
			EngineeringUnits.Temperature fluidTemp){

		this.rangeCheck(fluidTemp);
		// note, specific entropy and heat capcity are the same unit...
		//
		double cp;
		cp = 1518 + 2.82*fluidTemp.As(TemperatureUnit.DegreeCelsius);

		return new SpecificHeatCapacity(cp,
				SpecificHeatCapacityUnit.JoulePerKilogramKelvin);
	}

	public ThermalConductivity getDowthermAThermalConductivity(
			EngineeringUnits.Temperature fluidTemp){


		this.rangeCheck(fluidTemp);
		double thermalConductivity;
		thermalConductivity = 0.142 - 0.00016* fluidTemp.As(TemperatureUnit.
				DegreeCelsius);
		return new ThermalConductivity(thermalConductivity,
				ThermalConductivityUnit.WattPerMeterKelvin);
	}

	
	public bool rangeCheck(EngineeringUnits.Temperature
			fluidTemp){
		double tempvalueCelsius;
		tempvalueCelsius = fluidTemp.As(TemperatureUnit.DegreeCelsius);

		this.rangeCheck(tempvalueCelsius);

		return true;
	}

	
	// this function checks if a fluid temperature falls in a range (20-180C)
	// it is assumed that temperature here is in degrees C
	// to avoid units, use the overload above.
	public bool rangeCheck(double fluidTemp){
		if(fluidTemp < 20.0){
			string errorMsg;
			errorMsg = "Your fluid temperature \n";
			errorMsg += "is too low :";
			errorMsg += fluidTemp.ToString();
			errorMsg += "C \n";
			errorMsg += "\n the minimum is 20C";

			throw new ArgumentException(errorMsg);
		}


		if(fluidTemp > 180.0){
			string errorMsg;
			errorMsg = "Your fluid temperature \n";
			errorMsg += "is too high :";
			errorMsg += fluidTemp.ToString();
			errorMsg += "C \n";
			errorMsg += "\n the minimum is 180C";

			throw new ArgumentException(errorMsg);
		}

		return true;

	}

}
