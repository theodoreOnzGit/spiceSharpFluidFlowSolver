using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using spiceSharpFluidFlowSolverLibraries;
using SharpFluids;


namespace therminolPipeTest;

public class TherminolComparisonTests : testOutputHelper
{
	public TherminolComparisonTests(ITestOutputHelper outputHelper):
		base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

    [Fact(Skip="sandbox")]
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

	// want to check if the object value reflects the vendor
	// value to within 2% for range of interest
	// 20-180C
	// https://www.therminol.com/sites/therminol/files/documents/TF09A_Therminol_VP1.pdf
	[Theory]
	[InlineData(20,1064)]
	[InlineData(30,1056)]
	[InlineData(40,1048)]
	[InlineData(50,1040)]
	[InlineData(60,1032)]
	[InlineData(70,1024)]
	[InlineData(80,1015)]
	[InlineData(90,1007)]
	[InlineData(100,999)]
	[InlineData(110,991)]
	[InlineData(120,982)]
	[InlineData(130,974)]
	[InlineData(140,965)]
	[InlineData(150,957)]
	[InlineData(160,948)]
	[InlineData(180,931)]
	public void WhenTherminolObjectTestedExpectVendorDensityValue(
			double temperatureC, double densityValueKgPerM3){

		//Setup


		// set temperature and pressure for dowtherm and Therminol
		Pressure referencePressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature 
			= new EngineeringUnits.Temperature(temperatureC, 
					TemperatureUnit.DegreeCelsius);

		// get therminol VP-1 fluid object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

		// Act
		therminol.UpdatePT(referencePressure, testTemperature);

		Density resultDensity = therminol.Density;

		// Assert 
		//
		// Check if densities are equal to within 0.2% of vendor data
	
		double errorMax = 0.2/100;
		double resultDensityValueKgPerM3 = resultDensity.
			As(DensityUnit.KilogramPerCubicMeter);
		double error = Math.Abs(resultDensityValueKgPerM3 - 
				densityValueKgPerM3)/densityValueKgPerM3;

		if (error < errorMax){
			return;
		}
		if (error > errorMax){

		Assert.Equal(densityValueKgPerM3, 
				resultDensity.As(DensityUnit.KilogramPerCubicMeter),
				0);
		}
		
	}

	// viscosity data"
	// https://www.therminol.com/sites/therminol/files/documents/TF09A_Therminol_VP1.pdf
	[Theory]
	[InlineData(20,4.29)]
	[InlineData(30,3.28)]
	[InlineData(40,2.60)]
	[InlineData(50,2.12)]
	[InlineData(60,1.76)]
	[InlineData(70,1.49)]
	[InlineData(80,1.28)]
	[InlineData(90,1.12)]
	[InlineData(100,0.985)]
	[InlineData(110,0.875)]
	[InlineData(120,0.784)]
	[InlineData(130,0.707)]
	[InlineData(140,0.642)]
	[InlineData(150,0.585)]
	[InlineData(160,0.537)]
	[InlineData(180,0.457)]
	public void WhenTherminolObjectTestedExpectVendorDynamicViscosityValue(
			double temperatureC, double viscosityRefValueCentiPoise){

		//Setup


		// set temperature and pressure for dowtherm and Therminol
		Pressure referencePressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature 
			= new EngineeringUnits.Temperature(temperatureC, 
					TemperatureUnit.DegreeCelsius);

		// get therminol VP-1 fluid object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

		// Act
		therminol.UpdatePT(referencePressure, testTemperature);

		DynamicViscosity resultDynamicViscosity = therminol.DynamicViscosity;

		// Assert 
		//
		// Check if densities are equal to within 3.5% of vendor data
	
		double errorMax = 3.5/100;
		double resultDynamicViscosityValueKgPerM3 = resultDynamicViscosity.
			As(DynamicViscosityUnit.Centipoise);
		double error = Math.Abs(resultDynamicViscosityValueKgPerM3 - 
				viscosityRefValueCentiPoise)/viscosityRefValueCentiPoise;

		if (error < errorMax){
			return;
		}
		if (error > errorMax){

		Assert.Equal(viscosityRefValueCentiPoise, 
				resultDynamicViscosity.As(DynamicViscosityUnit.Centipoise),
				0);
		}
		
	}

	[Theory]
	[InlineData(80,1.28)]
	[InlineData(90,1.12)]
	[InlineData(100,0.985)]
	[InlineData(110,0.875)]
	[InlineData(120,0.784)]
	public void ExpectTherminolObjectErrorLessThan1PercentWithinScalingRange(
			double temperatureC, double viscosityRefValueCentiPoise){

		//Setup


		// set temperature and pressure for dowtherm and Therminol
		Pressure referencePressure = new Pressure(1.1013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature 
			= new EngineeringUnits.Temperature(temperatureC, 
					TemperatureUnit.DegreeCelsius);

		// get therminol VP-1 fluid object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);

		// Act
		therminol.UpdatePT(referencePressure, testTemperature);

		DynamicViscosity resultDynamicViscosity = therminol.DynamicViscosity;

		// Assert 
		//
		// Check if densities are equal to within 0.1% of vendor data
		// within scaling range for flibe 80-120C 
	
		double errorMax = 0.1/100;
		double resultDynamicViscosityValueKgPerM3 = resultDynamicViscosity.
			As(DynamicViscosityUnit.Centipoise);
		double error = Math.Abs(resultDynamicViscosityValueKgPerM3 - 
				viscosityRefValueCentiPoise)/viscosityRefValueCentiPoise;

		if (error < errorMax){
			return;
		}
		if (error > errorMax){

		Assert.Equal(viscosityRefValueCentiPoise, 
				resultDynamicViscosity.As(DynamicViscosityUnit.Centipoise),
				0);
		}
		
	}

	// Prandtl Number data for Therminol VP-1
	// https://www.therminol.com/sites/therminol/files/documents/TF09A_Therminol_VP1.pdf
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
	public void WhenTemperatureVariedExpectTherminolAndDowthermPrandtlWithin25Percent(
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
		double errorMax = 25.0/100;
		double error = Math.Abs(testPrandtlNumber - 
				referencePrandtlNumber)/referencePrandtlNumber;

		if(error < errorMax){
			return;
		}
		if(error > errorMax){
		Assert.Equal(referencePrandtlNumber,testPrandtlNumber,
				1);
		}

	}

	// Prandtl Number data for Therminol VP-1
	// https://www.therminol.com/sites/therminol/files/documents/TF09A_Therminol_VP1.pdf
	// This is the same test for Prandtl number
	// but with a lower error tolerance in the range of scaling
	[Theory]
	[InlineData(80)]
	[InlineData(90)]
	[InlineData(100)]
	[InlineData(120)]
	public void ExpectDowthermAndTherminolPrandtlWithin10PercentInScalingRange(
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
		double errorMax = 9.5/100;
		double error = Math.Abs(testPrandtlNumber - 
				referencePrandtlNumber)/referencePrandtlNumber;

		if(error < errorMax){
			return;
		}
		if(error > errorMax){
		Assert.Equal(referencePrandtlNumber,testPrandtlNumber,
				1);
		}

	}


	// this test isn't actually a test, but just generates
	// the csv data of temperature in C vs dowtherm A Pr
	// and therminol Pr
	[Fact]
	public void generatePrandtlCSV(){

		List<double> temperatureList = new List<double>();
		for (int i = 2; i < 19; i++)
		{
			//i'm adding temperature in degrees C 
			//from 20 to 180 C
			temperatureList.Add(10.0*i);
		}

		// now i want to generate a csv file with prandtl
		// numbers of dowtherm A first,
		// then therminol
		//
		// first let me generate my therminol object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;

		// now let me generate a list of 
		// double, double, double
		// the first number being temperature in degC
		// second is Prandtl number in dowthermA
		// third is Prandtl number in Therminol VP1
		// this is a list of tuples.
		List<(double, double, double)> prandtlData =
			new List<(double,double,double)>();

		double dowthermAPrandtlNumber;
		double therminolVP1PrandtlNumber;
		foreach (double tempCValue in temperatureList)
		{
			
			// here i get my temperature object
			testTemperature = new EngineeringUnits.
				Temperature(tempCValue, 
						TemperatureUnit.DegreeCelsius);


			// then i get my dowtherm prandtl data
			dowthermAPrandtlNumber = this.getDowthermAViscosity(testTemperature) *
				this.getDowthermAConstantPressureHeatCapacity(testTemperature) / 
				this.getDowthermAThermalConductivity(testTemperature);

			// then i get my therminol prandtl number data
			therminol.UpdatePT(referencePressure, 
					testTemperature);
			therminol.UpdatePT(referencePressure, testTemperature);
			therminolVP1PrandtlNumber = therminol.Prandtl;

			// i'll make the dataset tuple
			(double, double, double) dataSet = 
				(tempCValue, dowthermAPrandtlNumber, therminolVP1PrandtlNumber);

			// then i add it to the prandtlData tuple list
			prandtlData.Add(dataSet);
		}

		// now it's time to write the code into a directory
		//
	
		string PrandtlDataSet ="";
		foreach (var dataSet in prandtlData)
		{
			string tempCString = dataSet.Item1.ToString();
			string dowthermPrandtlString = dataSet.Item2.ToString();
			string therminolPrandtlString = dataSet.Item3.ToString();

			PrandtlDataSet += tempCString + "," +
					dowthermPrandtlString + "," +
					therminolPrandtlString + "\n";
		}
		
		using (System.IO.StreamWriter csvFile = 
				new System.IO.StreamWriter("prandtlData.csv"))
		{
			csvFile.WriteLine(PrandtlDataSet);
		}


	}

	// this isn't a test
	// but generates a csv file of specific heat capacity 
	// of Dowtherm A vs therminol VP1
	// at 20 to 180C
	// units are SI 
	// J/kg/C
	[Fact]
	public void generateHeatCapacityCSV(){

		List<double> temperatureList = new List<double>();
		for (int i = 2; i < 19; i++)
		{
			//i'm adding temperature in degrees C 
			//from 20 to 180 C
			temperatureList.Add(10.0*i);
		}

		// now i want to generate a csv file with prandtl
		// numbers of dowtherm A first,
		// then therminol
		//
		// first let me generate my therminol object
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;

		// now let me generate a list of 
		// double, double, double
		// the first number being temperature in degC
		// second is heatCapacity number in dowthermA
		// third is heatCapacity number in Therminol VP1
		// this is a list of tuples.
		List<(double, double, double)> heatCapacityData =
			new List<(double,double,double)>();

		double dowthermAheatCapacityNumber;
		double therminolVP1heatCapacityNumber;
		foreach (double tempCValue in temperatureList)
		{
			
			// here i get my temperature object
			testTemperature = new EngineeringUnits.
				Temperature(tempCValue, 
						TemperatureUnit.DegreeCelsius);


			// then i get my dowtherm heatCapacity data
			dowthermAheatCapacityNumber = 
				this.getDowthermAConstantPressureHeatCapacity(testTemperature).
				As(SpecificHeatCapacityUnit.JoulePerKilogramKelvin);

			// then i get my therminol heatCapacity number data
			therminol.UpdatePT(referencePressure, 
					testTemperature);
			therminol.UpdatePT(referencePressure, testTemperature);
			therminolVP1heatCapacityNumber = therminol.Cp.As(
					SpecificHeatCapacityUnit.JoulePerKilogramKelvin);

			// i'll make the dataset tuple
			(double, double, double) dataSet = 
				(tempCValue, dowthermAheatCapacityNumber, therminolVP1heatCapacityNumber);

			// then i add it to the heatCapacityData tuple list
			heatCapacityData.Add(dataSet);
		}

		// now it's time to write the code into a directory
		//
	
		string heatCapacityDataSet ="tempC, dowthermA SpecificHeatCapacity " +
			"JoulePerKilogramKelvin," +
			"therminolVP1 SpecificHeatCapacity JoulePerKilogramKelvin"  +
		   "\n";
		foreach (var dataSet in heatCapacityData)
		{
			string tempCString = dataSet.Item1.ToString();
			string dowthermheatCapacityString = dataSet.Item2.ToString();
			string therminolheatCapacityString = dataSet.Item3.ToString();

			heatCapacityDataSet += tempCString + "," +
					dowthermheatCapacityString + "," +
					therminolheatCapacityString + "\n";
		}
		
		using (System.IO.StreamWriter csvFile = 
				new System.IO.StreamWriter("heatCapacityData.csv"))
		{
			csvFile.WriteLine(heatCapacityDataSet);
		}


	}



	// the following section contains correlations for 
	// Dowtherm A
	//
	// 
	//
	//
	//
	//


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
			errorMsg += "\n the max is 180C";

			throw new ArgumentException(errorMsg);
		}

		return true;

	}

}
