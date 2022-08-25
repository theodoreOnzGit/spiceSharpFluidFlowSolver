using EngineeringUnits;
using EngineeringUnits.Units;
using SharpFluids;
using Xunit;
using System;
using Xunit.Abstractions;


namespace therminolPipeTest;

public class UnitTest2 
{

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


}
