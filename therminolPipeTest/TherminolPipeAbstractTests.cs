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
	[InlineData(1,25.0)]
	[InlineData(2,30.5)]
	[InlineData(3, 65.6)]
	[InlineData(4, 122.8)]
	[InlineData(5, 80.5)]
	public void WhenSetTemperatureListExpectCorrectViscosity(
			int numberOfSegments,
			double temperatureValC){
		// Setup
		// let's first get the expected viscosity:

		DynamicViscosity expectedFluidDynamicViscosity
			(EngineeringUnits.Temperature fluidTemp){
			Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
			Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
			therminol.UpdatePT(referencePressure, fluidTemp);
			return therminol.DynamicViscosity.ToUnit(
					DynamicViscosityUnit. PascalSecond);
		}

		DynamicViscosity expectedDynamicViscosity = expectedFluidDynamicViscosity(
				new EngineeringUnits.Temperature(temperatureValC,
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
		// now let's get the viscosityList

		IList<DynamicViscosity> testDynamicViscosityList 
			= testPipe.viscosityList;

		// And assert everything
		foreach (DynamicViscosity segmentDynamicViscosity in testDynamicViscosityList){
			Assert.Equal(expectedDynamicViscosity.
					As(DynamicViscosityUnit.PascalSecond),
					segmentDynamicViscosity.
					As(DynamicViscosityUnit.PascalSecond));
		}
	}


	[Theory]
	[InlineData(1,25.0)]
	[InlineData(2,30.5)]
	[InlineData(3, 65.6)]
	[InlineData(4, 122.8)]
	[InlineData(5, 80.5)]
	public void WhenSetTemperatureListExpectCorrectThermalConductivity(
			int numberOfSegments,
			double temperatureValC){
		// Setup
		// let's first get the expected thermalConductivity:

		ThermalConductivity expectedFluidThermalConductivity
			(EngineeringUnits.Temperature fluidTemp){
			Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
			Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
			therminol.UpdatePT(referencePressure, fluidTemp);
			return therminol.Conductivity.ToUnit(
					ThermalConductivityUnit.WattPerMeterKelvin);
		}

		ThermalConductivity expectedThermalConductivity = expectedFluidThermalConductivity(new 
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
		// now let's get the thermalConductivityList

		IList<ThermalConductivity> testThermalConductivityList 
			= testPipe.thermalConductivityList;

		// And assert everything
		foreach (ThermalConductivity segmentThermalConductivity in testThermalConductivityList){
			Assert.Equal(expectedThermalConductivity.
					As(ThermalConductivityUnit.WattPerMeterKelvin),
					segmentThermalConductivity.
					As(ThermalConductivityUnit.WattPerMeterKelvin));
		}
	}
	
	[Theory]
	[InlineData(1,25.0)]
	[InlineData(2,30.5)]
	[InlineData(3, 65.6)]
	[InlineData(4, 122.8)]
	[InlineData(5, 80.5)]
	public void WhenSetTemperatureListExpectCorrectSpecificHeat(
			int numberOfSegments,
			double temperatureValC){
		// Setup
		// let's first get the expected heatCapacity:

		SpecificHeatCapacity expectedFluidSpecificHeatCapacity
			(EngineeringUnits.Temperature fluidTemp){
			Fluid therminol = new Fluid(FluidList.InCompTherminolVP1);
			Pressure referencePressure = new Pressure(1.013e5, PressureUnit.Pascal);
			therminol.UpdatePT(referencePressure, fluidTemp);
			return therminol.Cp.ToUnit(
					SpecificHeatCapacityUnit.JoulePerKilogramKelvin);
		}

		SpecificHeatCapacity expectedSpecificHeatCapacity = expectedFluidSpecificHeatCapacity(new 
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

		IList<SpecificHeatCapacity> testSpecificHeatCapacityList 
			= testPipe.heatCapacityList;

		// And assert everything
		foreach (SpecificHeatCapacity segmentSpecificHeatCapacity in testSpecificHeatCapacityList){
			Assert.Equal(expectedSpecificHeatCapacity.
					As(SpecificHeatCapacityUnit.JoulePerKilogramKelvin),
					segmentSpecificHeatCapacity.
					As(SpecificHeatCapacityUnit.JoulePerKilogramKelvin));
		}
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
		// let's first get the expected temperature:


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
	/******************************************
	 *
	 * This next series of tests checks if i am able to set
	 * a non uniform temperature across the pipe
	 * and then when i do, i wanted to check if i am able to
	 * get the correct thermodynamic properties
	 *
	 *
	 * *******************************************/

	// this first test checks if setting the temperature  list
	// works properly
	// 
	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(4, false)]
	[InlineData(5, true)]
	public void WhenTemperatureListNonUniformExpectCorrectTemperature(
			int numberOfSegments,
			bool increaseTemperature){
		// Setup
		// let's first get the expected temperature:


		EngineeringUnits.Temperature referenceTemperature =
			new EngineeringUnits.Temperature(70.0,
					TemperatureUnit.DegreeCelsius);
		// next let's setup our testpipe and set the
		// temperature to a non uniform temperature
		//
		// First i make the test pipe


		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(0.5, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		// then i make a reference temperatureList
		//
		IList<EngineeringUnits.Temperature> 
			referenceTemperatureList 
			= new List<EngineeringUnits.Temperature>();

		// then i want a varying temperature profile,
		// which i can 
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			if(increaseTemperature == true){
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue += 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}else{
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue -= 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}

		}

		testPipe.temperatureList = referenceTemperatureList;
		// Act
		// now let's get the testTemperatureList

		IList<EngineeringUnits.Temperature> testTemperatureList = 
			testPipe.temperatureList;

		// And assert everything
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			Assert.Equal(
					referenceTemperatureList[segmentNumber-1].As(
						TemperatureUnit.DegreeCelsius),
					testTemperatureList[segmentNumber-1].As(
						TemperatureUnit.DegreeCelsius));
			EngineeringUnits.Temperature segmentTemperature;
			segmentTemperature = testTemperatureList[
				segmentNumber-1];
		}

	}

	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(4, false)]
	[InlineData(5, true)]
	public void WhenTemperatureListNonUniformExpectCorrectViscosity(
			int numberOfSegments,
			bool increaseTemperature){
		// Setup
		// let's first get the expected temperature:


		EngineeringUnits.Temperature referenceTemperature =
			new EngineeringUnits.Temperature(70.0,
					TemperatureUnit.DegreeCelsius);
		// next let's setup our testpipe and set the
		// temperature to a non uniform temperature
		//
		// First i make the test pipe


		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(0.5, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		// then i make a reference temperatureList
		//
		IList<EngineeringUnits.Temperature> 
			referenceTemperatureList 
			= new List<EngineeringUnits.Temperature>();

		// then i want a varying temperature profile,
		// which i can 
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			if(increaseTemperature == true){
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue += 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}else{
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue -= 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}

		}

		testPipe.temperatureList = referenceTemperatureList;

		// now let's make a reference thermodynamic property list

		IList<DynamicViscosity> getRefDynamicViscosityList(
				IList<EngineeringUnits.Temperature> 
				temperatureList){
			IList<DynamicViscosity> dynamicViscosityList = 
				new List<DynamicViscosity>();

			foreach(EngineeringUnits.Temperature 
					segmentTemperature in temperatureList){
				dynamicViscosityList.Add(
						testPipe.getFluidDynamicViscosity(
							segmentTemperature));


			}
			return dynamicViscosityList;
		}

		IList<DynamicViscosity> refDynamicViscosityList = 
			getRefDynamicViscosityList(
					referenceTemperatureList);



		// Act
		// now let's get the testTemperatureList

		IList<DynamicViscosity> testDynamicViscosityList = 
			testPipe.viscosityList;

		// And assert everything
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			Assert.Equal(
					refDynamicViscosityList[segmentNumber-1].
					As(DynamicViscosityUnit.PascalSecond),
					testDynamicViscosityList[segmentNumber-1].
					As(DynamicViscosityUnit.PascalSecond));
		}

	}

	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(4, false)]
	[InlineData(5, true)]
	public void WhenTemperatureListNonUniformExpectCorrectDensity(
			int numberOfSegments,
			bool increaseTemperature){
		// Setup
		// let's first get the expected temperature:


		EngineeringUnits.Temperature referenceTemperature =
			new EngineeringUnits.Temperature(70.0,
					TemperatureUnit.DegreeCelsius);
		// next let's setup our testpipe and set the
		// temperature to a non uniform temperature
		//
		// First i make the test pipe


		TherminolPipe testPipe = 
			new mockTherminolPipe("mockTherminolPipe", "0","out");

		testPipe.componentLength = new Length(0.5, LengthUnit.Meter);
		testPipe.numberOfSegments = numberOfSegments;
		// then i make a reference temperatureList
		//
		IList<EngineeringUnits.Temperature> 
			referenceTemperatureList 
			= new List<EngineeringUnits.Temperature>();

		// then i want a varying temperature profile,
		// which i can 
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			if(increaseTemperature == true){
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue += 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}else{
				EngineeringUnits.Temperature
					segmentTemperature;
				double segementTempCValue = 
					referenceTemperature.As(TemperatureUnit.
							DegreeCelsius);
				segementTempCValue -= 5.0*(segmentNumber-1);
				segmentTemperature = new Temperature(
						segementTempCValue, TemperatureUnit.
						DegreeCelsius);
				referenceTemperatureList.Add(
						segmentTemperature);
			}

		}

		testPipe.temperatureList = referenceTemperatureList;

		// now let's make a reference thermodynamic property list

		IList<Density> getRefDensityList(
				IList<EngineeringUnits.Temperature> 
				temperatureList){
			IList<Density> dynamicViscosityList = 
				new List<Density>();

			foreach(EngineeringUnits.Temperature 
					segmentTemperature in temperatureList){
				dynamicViscosityList.Add(
						testPipe.getFluidDensity(
							segmentTemperature));


			}
			return dynamicViscosityList;
		}

		IList<Density> refDensityList = 
			getRefDensityList(
					referenceTemperatureList);



		// Act
		// now let's get the testTemperatureList

		IList<Density> testDensityList = 
			testPipe.densityList;

		// And assert everything
		for(int segmentNumber = 1;
				segmentNumber <= numberOfSegments;
				segmentNumber++){
			Assert.Equal(
					refDensityList[segmentNumber-1].
					As(DensityUnit.KilogramPerCubicMeter),
					testDensityList[segmentNumber-1].
					As(DensityUnit.KilogramPerCubicMeter));

		}

	}
}
