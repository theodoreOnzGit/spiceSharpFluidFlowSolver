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
 
	// here i need to test the therminol pipe abstract class for five things
	// 1) double Pr
	// 2) SpecificHeatCapacity Cp
	// 3) Density rho
	// 4) DynamicViscosity mu
	// 5) ThermalConductivity k
	//
	// Test (1/5)
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
	public void WhenAbstractTherminolPipeTemperatureVariedExpectPrandtl(
			double tempCValue){
		// this test checks if the functions returning prandtl number
		// from the therminol pipe abstract class 
		// will return the correct prandtl number
		
		
		// Setup
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;
		testTemperature = new EngineeringUnits.
			Temperature(tempCValue, TemperatureUnit.DegreeCelsius);

		therminol.UpdatePT(referencePressure, testTemperature);
		double referencePrandtlNumber = therminol.Prandtl;
		double testPrandtlNumber;
		// let's make  a mockTherminolPipe which inherits from the 
		// therminolPipe Abstract class but implements all methods with
		// throw new NotImplementedException()

		TherminolPipe testPipe = new mockTherminolPipe("mockTherminolPipe",
				"0","out");

		testPrandtlNumber = testPipe.getFluidPrandtl(testTemperature);

		// Act


		// Assert
		//
		Assert.Equal(referencePrandtlNumber, testPrandtlNumber);

	}

	//
	// here i need to test the therminol pipe abstract class for five things
	// 1) double Pr
	// 2) SpecificHeatCapacity Cp
	// 3) Density rho
	// 4) DynamicViscosity mu
	// 5) ThermalConductivity k
	//
	// Test (2/5)
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
	public void WhenAbstractTherminolPipeTemperatureVariedExpectHeatCapacity(
			double tempCValue){
		// this test checks if the functions returning prandtl number
		// from the therminol pipe abstract class 
		// will return the correct prandtl number
		
		
		// Setup
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;
		testTemperature = new EngineeringUnits.
			Temperature(tempCValue, TemperatureUnit.DegreeCelsius);

		therminol.UpdatePT(referencePressure, testTemperature);
		double referenceSpecificHeatCapacity = therminol.Cp.As(
				SpecificHeatCapacityUnit.JoulePerKilogramKelvin);
		double testSpecificHeatCapacity;
		// let's make  a mockTherminolPipe which inherits from the 
		// therminolPipe Abstract class but implements all methods with
		// throw new NotImplementedException()

		TherminolPipe testPipe = new mockTherminolPipe("mockTherminolPipe",
				"0","out");

		testSpecificHeatCapacity = testPipe.getFluidHeatCapacity(
				testTemperature).As(
					SpecificHeatCapacityUnit.JoulePerKilogramKelvin);

		// Act


		// Assert
		//
		Assert.Equal(referenceSpecificHeatCapacity, testSpecificHeatCapacity);

	}


	// here i need to test the therminol pipe abstract class for five things
	// 1) double Pr
	// 2) SpecificHeatCapacity Cp
	// 3) Density rho
	// 4) DynamicViscosity mu
	// 5) ThermalConductivity k
	//
	// Test (3/5)
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
	public void WhenAbstractTherminolPipeTemperatureVariedExpectDensity(
			double tempCValue){
		// this test checks if the functions returning prandtl number
		// from the therminol pipe abstract class 
		// will return the correct prandtl number
		
		
		// Setup
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;
		testTemperature = new EngineeringUnits.
			Temperature(tempCValue, TemperatureUnit.DegreeCelsius);

		therminol.UpdatePT(referencePressure, testTemperature);
		double referenceDensity = therminol.Density.As(
				DensityUnit.KilogramPerCubicMeter);
		double testDensity;
		// let's make  a mockTherminolPipe which inherits from the 
		// therminolPipe Abstract class but implements all methods with
		// throw new NotImplementedException()

		TherminolPipe testPipe = new mockTherminolPipe("mockTherminolPipe",
				"0","out");

		testDensity = testPipe.getFluidDensity(
				testTemperature).As(DensityUnit.KilogramPerCubicMeter);

		// Act


		// Assert
		//
		Assert.Equal(referenceDensity, testDensity);

	}


	// here i need to test the therminol pipe abstract class for five things
	// 1) double Pr
	// 2) SpecificHeatCapacity Cp
	// 3) Density rho
	// 4) DynamicViscosity mu
	// 5) ThermalConductivity k
	//
	// Test (4/5)
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
	public void WhenAbstractTherminolPipeTemperatureVariedExpectDynamicVis(
			double tempCValue){
		// this test checks if the functions returning prandtl number
		// from the therminol pipe abstract class 
		// will return the correct prandtl number
		
		
		// Setup
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;
		testTemperature = new EngineeringUnits.
			Temperature(tempCValue, TemperatureUnit.DegreeCelsius);

		therminol.UpdatePT(referencePressure, testTemperature);
		double referenceDynamicViscosity = therminol.DynamicViscosity.As(
				DynamicViscosityUnit.PascalSecond);
		double testDynamicViscosity;
		// let's make  a mockTherminolPipe which inherits from the 
		// therminolPipe Abstract class but implements all methods with
		// throw new NotImplementedException()

		TherminolPipe testPipe = new mockTherminolPipe("mockTherminolPipe",
				"0","out");

		testDynamicViscosity = testPipe.getFluidDynamicViscosity(
				testTemperature).As(DynamicViscosityUnit.PascalSecond);

		// Act


		// Assert
		//
		Assert.Equal(referenceDynamicViscosity, testDynamicViscosity);

	}


	// here i need to test the therminol pipe abstract class for five things
	// 1) double Pr
	// 2) SpecificHeatCapacity Cp
	// 3) Density rho
	// 4) DynamicViscosity mu
	// 5) ThermalConductivity k
	//
	// Test (5/5)
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
	public void WhenAbstractTherminolPipeTemperatureVariedExpectConductivity(
			double tempCValue){
		// this test checks if the functions returning prandtl number
		// from the therminol pipe abstract class 
		// will return the correct prandtl number
		
		
		// Setup
		Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
		Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
		EngineeringUnits.Temperature testTemperature;
		testTemperature = new EngineeringUnits.
			Temperature(tempCValue, TemperatureUnit.DegreeCelsius);

		therminol.UpdatePT(referencePressure, testTemperature);
		double referenceThermalConductivity = therminol.Conductivity.As(
				ThermalConductivityUnit.WattPerMeterKelvin);
		double testThermalConductivity;
		// let's make  a mockTherminolPipe which inherits from the 
		// therminolPipe Abstract class but implements all methods with
		// throw new NotImplementedException()

		TherminolPipe testPipe = new mockTherminolPipe("mockTherminolPipe",
				"0","out");

		testThermalConductivity = testPipe.getFluidThermalConductivity(
				testTemperature).As(ThermalConductivityUnit.
					WattPerMeterKelvin);

		// Act


		// Assert
		//
		Assert.Equal(referenceThermalConductivity, testThermalConductivity);

	}



	/***************************************************************
	 * The following section outlines tests for nodalisation
	 *
	 *
	 *
	 *
	 * ************************************************************/

	[Fact]
	public void WhenNumberOfNodesSetExpectEqualLength(){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(1.0, LengthUnit.Meter);
		testPipe.numberOfSegments = 10;
		// now let's retrieve the length list

		IList<Length> testPipeLengthList = new List<Length>();

		foreach (Length segmentLength in testPipe.lengthList)
		{
			testPipeLengthList.Add(segmentLength);
		}

		// so let me just get the first length off this list
		Length firstLength = testPipeLengthList[0];

		// Act

		// then i'll go through a for loop whether the legnths are
		// equal, if equal i will add to an integer known as the checksum
		// if the interger in the checksum is equal to the 
		// number of nodes, then the test passes
		//
		int checksum = 0;

		foreach (Length segmentLength in testPipeLengthList)
		{
			if(firstLength.As(LengthUnit.Meter) ==
					segmentLength.As(LengthUnit.Meter)){
				checksum++;
			}

		}
		// Assert
		//
		Assert.Equal(testPipe.numberOfSegments,checksum);
	}



	[Theory]
	[InlineData(5,1.0)]
	[InlineData(50,10.0)]
	[InlineData(70,1.0)]
	[InlineData(1,2.0)]
	public void WhenNumberOfNodesSetLengthEqual1dividebyNoOfSegments(
			int numberOfSegments, double componentLength){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(componentLength, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		Length expectedLength = testPipe.componentLength/numberOfSegments;
		// now let's retrieve the length list

		IList<Length> testPipeLengthList = new List<Length>();

		foreach (Length segmentLength in testPipe.lengthList)
		{
			testPipeLengthList.Add(segmentLength);
		}

		// so let me just get the first length off this list
		Length firstLength = testPipeLengthList[0];

		// Act

		// then i'll go through a for loop whether the legnths are
		// equal, if equal i will add to an integer known as the checksum
		// if the interger in the checksum is equal to the 
		// number of nodes, then the test passes
		//

		foreach (Length segmentLength in testPipeLengthList)
		{
			// now i know for each length i'm not supposed to use
			// so many assert.Equal in one test
			// but i want the test to fail if even one of the lengths 
			// isn't equal, so that's why i do it this way
			// the lazy way
			Assert.Equal(expectedLength.As(LengthUnit.Meter),
					segmentLength.As(LengthUnit.Meter));
		}
		// Assert
		//
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void WhenZeroSegmentsSetExpectDivideByZeroException(
			int numberOfSegments){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(10.0, LengthUnit.Meter);

		Assert.Throws<DivideByZeroException>(() => 
				testPipe.numberOfSegments = numberOfSegments);

	}


	[Theory]
	[InlineData(5,1.0)]
	[InlineData(50,10.0)]
	[InlineData(70,1.0)]
	[InlineData(1,2.0)]
	public void WhenNumberOfNodesSetExpectCorrectSegmentLength(
			int numberOfSegments, double componentLength){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(componentLength, LengthUnit.Meter);
		// Act
		testPipe.numberOfSegments = numberOfSegments;

		Length expectedDiameter = testPipe.entranceHydraulicDiameter;

		IList<Length> diameterList = testPipe.hydraulicDiameterList;

		// Assert
		foreach (Length diameter in diameterList)
		{
			Assert.Equal(expectedDiameter.As(LengthUnit.Meter)
					, diameter.As(LengthUnit.Meter));
		}

	}

	[Theory]
	[InlineData(5,1.0,1.5)]
	[InlineData(50,10.0,2.0)]
	[InlineData(70,1.0,0.5)]
	[InlineData(1,2.0,0.9)]
	public void WhenUnequalDiametersSetExpectCorrectCount(
			int numberOfSegments, double componentLength,
			double expansionRatio){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(componentLength, LengthUnit.Meter);

		// Act
		testPipe.numberOfSegments = numberOfSegments;

		testPipe.exitHydraulicDiameter = testPipe.entranceHydraulicDiameter *
			expansionRatio;



		IList<Length> diameterList = testPipe.hydraulicDiameterList;

		Assert.Equal(numberOfSegments,diameterList.Count);

	}

	[Theory]
	[InlineData(5,1.0,1.5)]
	[InlineData(50,10.0,2.0)]
	[InlineData(70,1.0,0.5)]
	[InlineData(1,2.0,0.9)]
	public void WhenUnequalDiametersSetExpectCorrectDiameter(
			int numberOfSegments, double componentLength,
			double expansionRatio){

		// Setup
		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(componentLength, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		testPipe.exitHydraulicDiameter = testPipe.entranceHydraulicDiameter *
			expansionRatio;

		// here i return the expected diameter given a segment number
		Length getExpectedDiameter(int segmentNumber){
			Length interpolationLength = 
				testPipe.getComponentLength()/
				numberOfSegments 
				*(segmentNumber - 0.5);

			double interpolationSlope;
			interpolationSlope = (testPipe.exitHydraulicDiameter 
					- testPipe.entranceHydraulicDiameter)/(
						testPipe.getComponentLength() - 
						testPipe.entranceLengthValue);


			return (interpolationLength - 
					testPipe.entranceLengthValue)*interpolationSlope
				+ testPipe.entranceHydraulicDiameter;
		}



		// Act

		
		IList<Length> diameterList = testPipe.hydraulicDiameterList;

		void printDiameterList(IList<Length> diameterList){
						foreach (var item in diameterList)
			{
				this.cout(item.ToString());
			}
			return;
		}

		// Assert

		for (int segmentNumber = 1; 
				segmentNumber <= numberOfSegments; 
				segmentNumber++
				)
		{
			
			Length expectedDiameter = getExpectedDiameter(segmentNumber);
			if(expectedDiameter.As(LengthUnit.Meter) !=
					diameterList[segmentNumber-1].As(LengthUnit.Meter)){
				printDiameterList(diameterList);
			}
			Assert.Equal(expectedDiameter.As(LengthUnit.Meter)
					,diameterList[segmentNumber-1].As(LengthUnit.Meter));
		}

	}

	[Theory]
	[InlineData()]
	public void WhenGetHydraulicDiameterExpectCorrectAverage(){
		throw new NotImplementedException();
	}

	[Theory]
	[InlineData(1,25.0)]
	[InlineData(2,30.5)]
	[InlineData(3, 65.6)]
	[InlineData(4, 122.8)]
	[InlineData(5, 80.5)]
	public void WhenSetTemperatureListExpectCorrectDensity(
			int numberOfSegments,
			double temperatureValC){
		// Setup
		// let's first get the expected density:

		Density expectedFluidDensity(EngineeringUnits.Temperature 
				fluidTemp){
			Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
			Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
			therminol.UpdatePT(referencePressure, fluidTemp);
			return therminol.Density.ToUnit(DensityUnit.KilogramPerCubicMeter);
		}

		Density expectedDensity = expectedFluidDensity(new 
				EngineeringUnits.Temperature(temperatureValC,
					TemperatureUnit.DegreeCelsius));
		// next let's setup our testpipe and set the
		// temperature to a uniform temperature

		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(0.5, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		testPipe.setTemperatureList(
				new EngineeringUnits.Temperature(
					temperatureValC, TemperatureUnit.DegreeCelsius));

		// Act
		// now let's get the densityList

		IList<Density> testDensityList = testPipe.densityList;

		// And assert everything
		foreach (Density segmentDensity in testDensityList){
			Assert.Equal(expectedDensity.As(DensityUnit.KilogramPerCubicMeter),
					segmentDensity.As(DensityUnit.KilogramPerCubicMeter));
		}


	}


	[Theory]
	[InlineData()]
	public void WhenSetTemperatureListExpectCorrectViscosity(){
		throw new NotImplementedException();
	}


	[Theory]
	[InlineData()]
	public void WhenSetTemperatureListExpectCorrectThermalConductivity(){
		throw new NotImplementedException();
	}
	
	[Theory]
	[InlineData()]
	public void WhenSetTemperatureListExpectCorrectSpecificHeatCapacity(){
		throw new NotImplementedException();
	}

	[Theory]
	[InlineData(1,25.0)]
	[InlineData(2,30.5)]
	[InlineData(3, 65.6)]
	[InlineData(4, 122.8)]
	[InlineData(5, 80.5)]
	public void WhenSetTemperatureListExpectCorrectTemperature(
			int numberOfSegments,
			double temperatureValC){
		// Setup
		// let's first get the expected density:


		EngineeringUnits.Temperature expectedTemperature =
			new EngineeringUnits.Temperature(temperatureValC,
					TemperatureUnit.DegreeCelsius);
		// next let's setup our testpipe and set the
		// temperature to a uniform temperature

		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(0.5, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		testPipe.setTemperatureList(
				new EngineeringUnits.Temperature(
					temperatureValC, TemperatureUnit.DegreeCelsius));

		// Act
		// now let's get the densityList

		IList<EngineeringUnits.Temperature> testTemperatureList = 
			testPipe.temperatureList;

		// And assert everything
		foreach (EngineeringUnits.Temperature 
				segmentTemperature in testTemperatureList){
			Assert.Equal(expectedTemperature.As(TemperatureUnit.
						DegreeCelsius), 
					segmentTemperature.As(TemperatureUnit.DegreeCelsius));
		}


	}

	/*******************
	 * The following section has Fact tests,
	 * but they are primarily used for generating csv files.
	 *
	 *
	 *
	 *
	 *
	 *************************************************************/


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
	
		string PrandtlDataSet ="tempDegreeC , dowthermAPrandtlNumber , therminolVP1PrandtlNumber \n ";
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


	// this isn't a test, but rather generates a file
	// for dynamic viscosity data
	[Fact]
	public void generateDynamicViscosityCSV(){

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
		// second is dynamicViscosity number in dowthermA
		// third is dynamicViscosity number in Therminol VP1
		// this is a list of tuples.
		List<(double, double, double)> dynamicViscosityData =
			new List<(double,double,double)>();

		double dowthermAdynamicViscosityNumber;
		double therminolVP1dynamicViscosityNumber;
		foreach (double tempCValue in temperatureList)
		{
			
			// here i get my temperature object
			testTemperature = new EngineeringUnits.
				Temperature(tempCValue, 
						TemperatureUnit.DegreeCelsius);


			// then i get my dowtherm dynamicViscosity data
			dowthermAdynamicViscosityNumber = 
				this.getDowthermAViscosity(testTemperature).
				As(DynamicViscosityUnit.PascalSecond);

			// then i get my therminol dynamicViscosity number data
			therminol.UpdatePT(referencePressure, 
					testTemperature);
			therminol.UpdatePT(referencePressure, testTemperature);
			therminolVP1dynamicViscosityNumber = therminol.DynamicViscosity.As(
					DynamicViscosityUnit.PascalSecond);

			// i'll make the dataset tuple
			(double, double, double) dataSet = 
				(tempCValue, dowthermAdynamicViscosityNumber, 
				 therminolVP1dynamicViscosityNumber);

			// then i add it to the dynamicViscosityData tuple list
			dynamicViscosityData.Add(dataSet);
		}

		// now it's time to write the code into a directory
		//
	
		string dynamicViscosityDataSet ="tempC, dowthermA DynamicViscosity " +
			"PascalSecond," +
			"therminolVP1 DynamicViscosity PascalSecond"  +
		   "\n";
		foreach (var dataSet in dynamicViscosityData)
		{
			string tempCString = dataSet.Item1.ToString();
			string dowthermdynamicViscosityString = dataSet.Item2.ToString();
			string therminoldynamicViscosityString = dataSet.Item3.ToString();

			dynamicViscosityDataSet += tempCString + "," +
					dowthermdynamicViscosityString + "," +
					therminoldynamicViscosityString + "\n";
		}
		
		using (System.IO.StreamWriter csvFile = 
				new System.IO.StreamWriter("dynamicViscosityData.csv"))
		{
			csvFile.WriteLine(dynamicViscosityDataSet);
		}


	}

	// this isn't a test but rather generates thermal conductivity
	// data for DowthermA and TherminolVP1 in a csv file
	//

	[Fact]
	public void generateThermalConductivityCSV(){

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
		// second is thermalConductivity number in dowthermA
		// third is thermalConductivity number in Therminol VP1
		// this is a list of tuples.
		List<(double, double, double)> thermalConductivityData =
			new List<(double,double,double)>();

		double dowthermAthermalConductivityNumber;
		double therminolVP1thermalConductivityNumber;
		foreach (double tempCValue in temperatureList)
		{
			
			// here i get my temperature object
			testTemperature = new EngineeringUnits.
				Temperature(tempCValue, 
						TemperatureUnit.DegreeCelsius);


			// then i get my dowtherm thermalConductivity data
			dowthermAthermalConductivityNumber = 
				this.getDowthermAThermalConductivity(testTemperature).
				As(ThermalConductivityUnit.WattPerMeterKelvin);

			// then i get my therminol thermalConductivity number data
			therminol.UpdatePT(referencePressure, 
					testTemperature);
			therminol.UpdatePT(referencePressure, testTemperature);
			therminolVP1thermalConductivityNumber = therminol.Conductivity.As(
					ThermalConductivityUnit.WattPerMeterKelvin);

			// i'll make the dataset tuple
			(double, double, double) dataSet = 
				(tempCValue, dowthermAthermalConductivityNumber, 
				 therminolVP1thermalConductivityNumber);

			// then i add it to the thermalConductivityData tuple list
			thermalConductivityData.Add(dataSet);
		}

		// now it's time to write the code into a directory
		//
	
		string thermalConductivityDataSet ="tempC, dowthermA ThermalConductivity " +
			"WattPerMeterKelvin," +
			"therminolVP1 ThermalConductivity WattPerMeterKelvin"  +
		   "\n";
		foreach (var dataSet in thermalConductivityData)
		{
			string tempCString = dataSet.Item1.ToString();
			string dowthermthermalConductivityString = dataSet.Item2.ToString();
			string therminolthermalConductivityString = dataSet.Item3.ToString();

			thermalConductivityDataSet += tempCString + "," +
					dowthermthermalConductivityString + "," +
					therminolthermalConductivityString + "\n";
		}
		
		using (System.IO.StreamWriter csvFile = 
				new System.IO.StreamWriter("thermalConductivityData.csv"))
		{
			csvFile.WriteLine(thermalConductivityDataSet);
		}


	}


	// this is not a test but rather generates a file of temperature
	// vs density for both Dowtherm A and TherminolVP1
	[Fact]
	public void generateDensityCSV(){

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
		// second is density number in dowthermA
		// third is density number in Therminol VP1
		// this is a list of tuples.
		List<(double, double, double)> densityData =
			new List<(double,double,double)>();

		double dowthermAdensityNumber;
		double therminolVP1densityNumber;
		foreach (double tempCValue in temperatureList)
		{
			
			// here i get my temperature object
			testTemperature = new EngineeringUnits.
				Temperature(tempCValue, 
						TemperatureUnit.DegreeCelsius);


			// then i get my dowtherm density data
			dowthermAdensityNumber = 
				this.getDowthermADensity(testTemperature).
				As(DensityUnit.KilogramPerCubicMeter);

			// then i get my therminol density number data
			therminol.UpdatePT(referencePressure, testTemperature);
			therminolVP1densityNumber = therminol.Density.As(
					DensityUnit.KilogramPerCubicMeter);

			// i'll make the dataset tuple
			(double, double, double) dataSet = (tempCValue, 
					dowthermAdensityNumber, 
				 therminolVP1densityNumber);

			// then i add it to the densityData tuple list
			densityData.Add(dataSet);
		}

		// now it's time to write the code into a directory
		//
	
		string densityDataSet ="tempC, dowthermA Density " +
			"KilogramPerCubicMeter," +
			"therminolVP1 Density KilogramPerCubicMeter"  +
		   "\n";
		foreach (var dataSet in densityData)
		{
			string tempCString = dataSet.Item1.ToString();
			string dowthermdensityString = dataSet.Item2.ToString();
			string therminoldensityString = dataSet.Item3.ToString();

			densityDataSet += tempCString + "," +
					dowthermdensityString + "," +
					therminoldensityString + "\n";
		}
		
		using (System.IO.StreamWriter csvFile = 
				new System.IO.StreamWriter("densityData.csv"))
		{
			csvFile.WriteLine(densityDataSet);
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
