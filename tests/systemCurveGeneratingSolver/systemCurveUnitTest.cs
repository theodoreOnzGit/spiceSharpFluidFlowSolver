using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public class systemCurveGeneratingSolver : testOutputHelper
{
	public systemCurveGeneratingSolver(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

	[Fact]
	public void sandbox_isothermalPipeSystemCurveSolver(){

		double pressureDrop = 1.45;

		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("out","0");
		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("out","0");
		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe,
				testPipe2,
				testPipe3
				);



		ISystemCurveSimulator steadyStateSim = 
			new systemCurveSimulator(
				"systemCurveSimulator");
		var currentExport1 = new RealPropertyExport(steadyStateSim, 
				"IsothermalPipe1", "i");
		var currentExport2 = new RealPropertyExport(steadyStateSim, 
				"IsothermalPipe2", "i");
		var currentExport3 = new RealPropertyExport(steadyStateSim, 
				"IsothermalPipe3", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current1 = -currentExport1.Value;
			var current2 = -currentExport2.Value;
			var current3 = -currentExport3.Value;
			steadyStateSim.simulationResult.Add(current1);
			steadyStateSim.simulationResult.Add(current2);
			steadyStateSim.simulationResult.Add(current3);

			Assert.Equal(2.1,current1);

		};
		steadyStateSim.Run(ckt);

		currentExport1.Destroy();
		currentExport2.Destroy();
		currentExport3.Destroy();
		// </example_customcomponent_nonlinearresistor_test>

		double massFlowRateTestValue = 0.0;
		foreach (double entry in steadyStateSim.simulationResult)
		{
			massFlowRateTestValue += entry;
		}
		MassFlow massFlowRateTestResult;
		massFlowRateTestResult = new MassFlow(massFlowRateTestValue,
				MassFlowUnit.SI);


		// Assert

		Assert.Equal(2.1, massFlowRateTestValue);
	}

	[Theory(Skip = "unstable, use other solver")]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(-1e-2)]
	[InlineData(1e-2)]
	[InlineData(0.0)]
	public void When_seriesSetupExpect3xFlowLength(
			double pressureDrop){

		// for this test i'm going to have 3 pipes in series
		// for the same pressure drop across 3 pipes in series
		// i should expect the same flowrate as 
		// a pipe 3 times as long

		// this step is needed to cast the testPipe as the
		// correct type
		//
		Component preCastPipe = new IsothermalPipe("isothermalPipe1");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("pumpOutlet","1");
		preCastPipe = new IsothermalPipe("isothermalPipe2");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("1","2");
		preCastPipe = new IsothermalPipe("isothermalPipe3");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("2","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "pumpOutlet", "0", pressureDrop),
				testPipe,
				testPipe2,
				testPipe3
				);


		// Setup the simulation and export our current

		double Be;
		Be = pressureDrop;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);

		double Re;
		ChurchillFrictionFactorJacobian _jacobianObject;
		_jacobianObject = new ChurchillFrictionFactorJacobian();
		double roughnessRatio = testPipe.Parameters.roughnessRatio();
		double lengthToDiameter = testPipe.Parameters.lengthToDiameter();
		// now for 3 pipes in series, my length is actually 3 times as long
		// so i need to multiply my L/D ratio by 3
		lengthToDiameter *= 3.0;
		Re = _jacobianObject.getRe(Be,roughnessRatio,lengthToDiameter);

		MassFlow massFlowRate;
		massFlowRate = testPipe.Parameters.fluidViscosity*
			testPipe.Parameters.crossSectionalArea()/
			testPipe.Parameters.hydraulicDiameter*
			Re;

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);





		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>

		double massFlowRateTestValue;
		massFlowRateTestValue = steadyStateSim.simulationResult;
		MassFlow massFlowRateTestResult;
		massFlowRateTestResult = new MassFlow(massFlowRateTestValue,
				MassFlowUnit.SI);

		this.cout("\n PrototypeSteadyStateFlowSimulation massFlowRateTestResult:" +
				massFlowRateTestResult.ToString());

		// Assert
		// 
		// Note that the Math.Abs is there because some massflowrates
		// are negative.
		// And also the massFlowRateTestResult are both
		// MassFlow objects, ie. dimensioned units
		// so i need to convert them to double using the .As()
		// method
		Assert.Equal(massFlowRate.As(MassFlowUnit.SI),
				massFlowRateTestResult.As(MassFlowUnit.SI),3);

		//throw new Exception();
	}

	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_parallelSetupExpect3xFlow(
			double pressureDrop){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate

		// this step is needed to cast the testPipe as the
		// correct type
		//
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("out","0");
		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("out","0");
		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe,
				testPipe2,
				testPipe3
				);

		// Setup the simulation and export our current

		double Be;
		Be = pressureDrop;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);

		double Re;
		ChurchillFrictionFactorJacobian _jacobianObject;
		_jacobianObject = new ChurchillFrictionFactorJacobian();
		double roughnessRatio = testPipe.Parameters.roughnessRatio();
		double lengthToDiameter = testPipe.Parameters.lengthToDiameter();
		Re = _jacobianObject.getRe(Be,roughnessRatio,lengthToDiameter);

		MassFlow massFlowRate;
		massFlowRate = testPipe.Parameters.fluidViscosity*
			testPipe.Parameters.crossSectionalArea()/
			testPipe.Parameters.hydraulicDiameter*
			Re;
		// now i multiply the flowrate by 3 so my expected flowrate
		// is 3x
		massFlowRate *= 3.0;

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);


		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>

		double massFlowRateTestValue;
		massFlowRateTestValue = steadyStateSim.simulationResult;
		MassFlow massFlowRateTestResult;
		massFlowRateTestResult = new MassFlow(massFlowRateTestValue,
				MassFlowUnit.SI);

		//this.cout("\n PrototypeSteadyStateFlowSimulation massFlowRateTestResult:" +
		//		massFlowRateTestResult.ToString());

		// Assert
		// 
		// Note that the Math.Abs is there because some massflowrates
		// are negative.
		// And also the massFlowRateTestResult are both
		// MassFlow objects, ie. dimensioned units
		// so i need to convert them to double using the .As()
		// method
		Assert.Equal(massFlowRate.As(MassFlowUnit.SI),
				massFlowRateTestResult.As(MassFlowUnit.SI),3);

		//throw new Exception();
	}
}
