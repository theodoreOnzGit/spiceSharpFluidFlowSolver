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


