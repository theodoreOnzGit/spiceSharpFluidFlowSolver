using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;
using System;
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
	[InlineData(1.0)]
	[InlineData(0.5)]
	[InlineData(0.1)]
	[InlineData(2.0)]
	[InlineData(3.0)]
	public void WhenFM40InSeries3x_Expect3xPressureDrop(
			double pressureDrop){

		// suppose i have a pressure drop of 3p m^2/s^2
		// this should produce a Reynolds number of Re
		// through a series of 3 flowmeters (height taken out)
		// and if i put the pressure drop P across one
		// flowmeter, i should get the same Re
		//
		//
		// Setup
		//

		// Here is the expected pressure drop
		double referenceRe;
		{
			IfLDKFactorGetRe getReObj = new flowmeterFM40();
			double hydraulicDiameter = 2.79e-2;
			KinematicViscosity kinViscosityObj= 
				new KinematicViscosity(4.03, 
						KinematicViscosityUnit.Centistokes);
			double kinViscosityValue = kinViscosityObj.As(
					KinematicViscosityUnit.SI);

			double BejanNumber = (pressureDrop/3.0)
				*Math.Pow(hydraulicDiameter,
						2.0);
			BejanNumber /= Math.Pow(kinViscosityValue,2.0);
			referenceRe = getReObj.getRe(BejanNumber);
		}


		FM40 FM40_1 = new FM40("flowmeter1");
		FM40_1.Connect("flowcircuitInlet","pt1");
		FM40_1.Parameters.inclineAngle =
			new Angle(0.0,AngleUnit.Degree);

		FM40 FM40_2 = new FM40("flowmeter2");
		FM40_2.Connect("pt1","pt2");
		FM40_2.Parameters.inclineAngle =
			new Angle(0.0,AngleUnit.Degree);

		FM40 FM40_3 = new FM40("flowmeter3");
		FM40_3.Connect("pt2","0");
		FM40_3.Parameters.inclineAngle =
			new Angle(0.0,AngleUnit.Degree);

		var ckt2 = new Circuit(
				new VoltageSource("V1", "flowcircuitInlet", "0", pressureDrop),
				FM40_1,
				FM40_2,
				FM40_3
				);

		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");


		steadyStateSim.simulationMode ="sourceStepping";
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");

		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(ckt2);

		// now i collect the results
		double massFlowValueKgPerS;
		massFlowValueKgPerS = steadyStateSim.simulationResult;

		// Re = massFlowrate/XSArea * hydraulicDiameter / kinViscosity
		double resultRe = massFlowValueKgPerS;
		resultRe *= FM40_1.Parameters.hydraulicDiameter.As(
				LengthUnit.SI);
		resultRe /= FM40_1.Parameters.crossSectionalArea().As(
				AreaUnit.SI);
		resultRe /= FM40_1.Parameters.fluidViscosity.As(
				DynamicViscosityUnit.SI);
		// Assert
		//
		// There are two ways of asserting here
		// one is where the error is less than 1%,
		// just assert true and return;
		//
		// otherwise show the actual number

		if( Math.Abs(1-referenceRe/resultRe) < 0.01){
			Assert.True(true);
			return;
		}
		Assert.Equal(referenceRe,
				resultRe,0);
	}

    [Fact]
    public void WhenFM40ComponentElevatedShouldEqualCorrelation()
    {
		//note: this test is here to see if the pressure drop
		// correlation holds if FM40 is raised to 90 degree
		// angle
		// Note: the length of the FM40 component is 0.36m
		// Setup
		double hydrostaticPressureIncrease =
			0.36*9.81*Math.Sin(90.0*Math.PI/180.0);
		double pressureDrop = 1;

		// here are our reference parameters
		IfLDKFactorGetRe getReObj = new flowmeterFM40();
		
		// now for a set Bejan number i should get a
		// set Reynold's number
		// let's see the Be
		// Be = deltaP * D^2/nu^2
		// These are parameters i can get within the flowmeter
		// parameters (i coded them in)
		// but i'll just set it manually for simplicity's sake
		// hydraulic diameter is in meters here
		double hydraulicDiameter = 2.79e-2;
		KinematicViscosity kinViscosityObj= 
			new KinematicViscosity(4.03, 
					KinematicViscosityUnit.Centistokes);
		double kinViscosityValue = kinViscosityObj.As(
				KinematicViscosityUnit.SI);

		double BejanNumber = (pressureDrop - hydrostaticPressureIncrease)
			*Math.Pow(hydraulicDiameter,
				2.0);
		BejanNumber /= Math.Pow(kinViscosityValue,2.0);

		double referenceRe = getReObj.getRe(BejanNumber);

		// here are the result parameters


		FM40 flowmeter = new FM40("flowmeter40");
		flowmeter.Connect("in","0");



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
		// Act
		steadyStateSim.Run(ckt2);

		// now i collect the results
		double massFlowValueKgPerS;
		massFlowValueKgPerS = steadyStateSim.simulationResult;

		// Re = massFlowrate/XSArea * hydraulicDiameter / kinViscosity
		double resultRe = massFlowValueKgPerS;
		resultRe *= flowmeter.Parameters.hydraulicDiameter.As(
				LengthUnit.SI);
		resultRe /= flowmeter.Parameters.crossSectionalArea().As(
				AreaUnit.SI);
		resultRe /= flowmeter.Parameters.fluidViscosity.As(
				DynamicViscosityUnit.SI);




		// Assert
		Assert.Equal(referenceRe,
				resultRe,3);
    }
	
    [Theory]
	[InlineData(1)]
    public void WhenFM40Component_FrictionFactorShouldEqualCorrelation(
			double pressureDrop)
    {
		// Setup

		// here are our reference parameters
		IfLDKFactorGetRe getReObj = new flowmeterFM40();
		
		// now for a set Bejan number i should get a
		// set Reynold's number
		// let's see the Be
		// Be = deltaP * D^2/nu^2
		// These are parameters i can get within the flowmeter
		// parameters (i coded them in)
		// but i'll just set it manually for simplicity's sake
		// hydraulic diameter is in meters here
		double hydraulicDiameter = 2.79e-2;
		KinematicViscosity kinViscosityObj= 
			new KinematicViscosity(4.03, 
					KinematicViscosityUnit.Centistokes);
		double kinViscosityValue = kinViscosityObj.As(
				KinematicViscosityUnit.SI);

		double BejanNumber = pressureDrop*Math.Pow(hydraulicDiameter,
				2.0);
		BejanNumber /= Math.Pow(kinViscosityValue,2.0);

		double referenceRe = getReObj.getRe(BejanNumber);

		// here are the result parameters


		FM40 flowmeter = new FM40("flowmeter40");
		flowmeter.Connect("0","in");
		flowmeter.Parameters.inclineAngle =
			new Angle(0.0,AngleUnit.Degree);



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
		// Act
		steadyStateSim.Run(ckt2);

		// now i collect the results
		double massFlowValueKgPerS;
		massFlowValueKgPerS = steadyStateSim.simulationResult;

		// Re = massFlowrate/XSArea * hydraulicDiameter / kinViscosity
		double resultRe = massFlowValueKgPerS;
		resultRe *= flowmeter.Parameters.hydraulicDiameter.As(
				LengthUnit.SI);
		resultRe /= flowmeter.Parameters.crossSectionalArea().As(
				AreaUnit.SI);
		resultRe /= flowmeter.Parameters.fluidViscosity.As(
				DynamicViscosityUnit.SI);




		// Assert
		Assert.Equal(referenceRe,
				resultRe,3);
    }

	[Theory]
	[InlineData(3.0)]
	public void WhenFM40InSeries_ExpectNonInfinity(
			double pressureDrop){

		FM40 FM40_1 = new FM40("flowmeter1");
		FM40_1.Connect("flowcircuitInlet","pt1");

		FM40 FM40_2 = new FM40("flowmeter2");
		FM40_2.Connect("pt1","pt2");

		FM40 FM40_3 = new FM40("flowmeter3");
		FM40_3.Connect("pt2","0");

		var ckt2 = new Circuit(
				new VoltageSource("V1", "0", "flowcircuitInlet", pressureDrop),
				FM40_1,
				FM40_2,
				FM40_3
				);

		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");


		steadyStateSim.simulationMode ="sourceStepping";
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");

		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(ckt2);

		if(steadyStateSim.simulationResult == double.NaN){
			string exceptionMsg = "NaN number";
			throw new Exception();
		}
		else if (steadyStateSim.simulationResult == double.NegativeInfinity | 
				steadyStateSim.simulationResult == double.PositiveInfinity)
		{
			string exceptionMsg = "Infinty";
			throw new Exception();
		}

		// Assert
		Assert.Equal(steadyStateSim.simulationResult,
				steadyStateSim.simulationResult);
	}
}


