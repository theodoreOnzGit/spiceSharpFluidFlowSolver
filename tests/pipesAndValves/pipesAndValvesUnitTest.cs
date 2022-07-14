using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public class pipesAndValvesUnitTest : testOutputHelper
{
	public pipesAndValvesUnitTest(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

	[Fact]
	public void sandboxForSeriesCircuitsMockPipe(){

		double voltage = 150;
		double resistance = 1e3;

		MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1");
		mockPipe.Connect("in","out");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 

		MockPipeCustomResistor mockPipe2 = new MockPipeCustomResistor("RNL2");
		mockPipe2.Connect("out","int");
		mockPipe2.Parameters.A = 2.0e3;
		mockPipe2.Parameters.B = 0.5; 

		MockPipeCustomResistor mockPipe3 = new MockPipeCustomResistor("RNL3");
		mockPipe3.Connect("int","0");
		mockPipe3.Parameters.A = 2.0e3;
		mockPipe3.Parameters.B = 0.5; 



		var ckt2 = new Circuit(
				new VoltageSource("V1", "in", "0", voltage),
				mockPipe,
				mockPipe2,
				mockPipe3
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
		steadyStateSim.Run(ckt2);

		// Assert
		Assert.Equal(0.0,
				steadyStateSim.simulationResult);

		
	}

	[Fact]
	public void sandboxForCircuitsSingleMockPipe(){

		double voltage = 150;
		double resistance = 1e3;

		MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1");
		mockPipe.Connect("in","0");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 




		var ckt2 = new Circuit(
				new VoltageSource("V1", "in", "0", voltage),
				mockPipe
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
		steadyStateSim.Run(ckt2);

		// Assert
		Assert.Equal(0.0,
				steadyStateSim.simulationResult);

		
	}

	[Fact]
	public void sandboxForSeriesCircuitsLinearResistor(){

		double voltage = 1.5;
		double resistance = 1e3;

		var cktB = new Circuit(
				new VoltageSource("V1", "in", "0", voltage),
				new Resistor("R1", "in", "out", resistance),
				new Resistor("R2", "out", "int", resistance),
				new Resistor("R3", "int", "0", resistance)
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
		steadyStateSim.Run(cktB);

		// Assert
		Assert.Equal(voltage/(3.0*resistance),
				steadyStateSim.simulationResult);

		
	}

	[Fact]
	public void sandbox_isothermalPipeWithCurrentSource(){


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

		double pressureDrop = 1.45;
		double currentExpected = -677.632;

		// Build the circuit
		var ckt = new Circuit(
				new CurrentSource("V1", "pumpOutlet", "0", currentExpected),
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
	[InlineData(1.45, 0.846910353, 0.0)]
	[InlineData(0.0, -0.846910353, 3660)]
	[InlineData(1.45, 180-0.846910353, 0.0)]
	[InlineData(0.0, 180+0.846910353, 3660)]
	public void WhenInclinedToZeroPressureDrop_ExpectNoFlow(
			double pressureDrop,
			double inclineAngleDegrees,
			double expectedFlowrateKgPerSecond){

		// Setup


		// for 10m long pipe, angle of incline for zero pressure drop
		// is about 0.84691 degrees
		Angle inclinedAngle = new Angle(inclineAngleDegrees, AngleUnit.Degree);
		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");
		testPipe.Parameters.inclineAngle = inclinedAngle;

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe
				);

		// build simulation
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
		steadyStateSim.Run(ckt);
		double massFlowrate = steadyStateSim.simulationResult;

		// Assert
		
		Assert.Equal(expectedFlowrateKgPerSecond, 
				massFlowrate,2);
	}

	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_extractDataFromPrototypeSimulation_expectNoError(
			double pressureDrop){


		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe
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
	public void When_ISteadyStateFlowSimulationAndPrototypeSimulationUsed_expectNoError(
			double pressureDrop){


		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe
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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference flowrate for PrototypeSteadyStateFlowSimulationTest is: " + 
				massFlowRate.ToString());




		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("PrototypeSteadyStateFlowSimulation: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}
	
	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_MockSteadyStateFlowSimulation_expectNoError(
			double pressureDrop){


		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe
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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference flowrate for MockSteadyStateFlowSimulationTest is: " + 
				massFlowRate.ToString());




		var steadyStateSim = new MockSteadyStateFlowSimulation(
				"MockSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("MockSteadyStateFlowSimulation: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}
	
	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_OperatingPoint_PrintResult(double pressureDrop){


		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe
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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference flowrate for steadyStateSimTest is: " + 
				massFlowRate.ToString());




		var steadyStateSim = new OP("OP");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("steadyStateSimTestOperatingPoint: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}

	[Fact]
	public void When_IsothermalPipeNegativePressureExpectNoError(){

		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				testPipe
				);

		// Setup the simulation and export our current
		double pressureDropMin;
		pressureDropMin = 1.45;

		double Be;
		Be = pressureDropMin;
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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference Mass flowrate is: " + 
				massFlowRate.ToString());




		var dc = new DC("DC", "V1", -1.5, -pressureDropMin, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("IsothermalPipe NegativePressure Verification: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}

	[Fact]
	public void When_IsothermalPipeGetDerivedQuantitesExpectNoError(){

		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				testPipe
				);

		// Setup the simulation and export our current
		double pressureDropMin;
		pressureDropMin = 1.45;

		double Be;
		Be = pressureDropMin;
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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference Mass flowrate is: " + 
				massFlowRate.ToString());




		var dc = new DC("DC", "V1", pressureDropMin, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("IsothermalPipe Verification: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}

	[Fact]
	public void When_BasePipeGetDerivedQuantitesExpectNoError(){

		PipeFactory pipeFactory = new PipeFactory("RNL1","out","0");
		Component preCastPipe = pipeFactory.returnPipe("MockPipeCustomResistor");
		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new BasePipe("basepipe1","out","0");
		BasePipe mockPipe = (BasePipe)preCasePipe;
		mockPipe.Connect("out","0");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		double pressureDropMin;
		pressureDropMin = 1.45;

		double Be;
		Be = pressureDropMin;
		Be *= mockPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be *= mockPipe.Parameters.pipeLength.
			As(LengthUnit.SI);
		Be /= mockPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);
		Be /= mockPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SI);

		double Re;
		ChurchillFrictionFactorJacobian _jacobianObject;
		_jacobianObject = new ChurchillFrictionFactorJacobian();
		double roughnessRatio = mockPipe.Parameters.roughnessRatio();
		double lengthToDiameter = mockPipe.Parameters.lengthToDiameter();
		Re = _jacobianObject.getRe(Be,roughnessRatio,lengthToDiameter);

		MassFlow massFlowRate;
		massFlowRate = mockPipe.Parameters.fluidViscosity*
			mockPipe.Parameters.crossSectionalArea()/
			mockPipe.Parameters.hydraulicDiameter*
			Re;

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

		this.cout("\n The reference Mass flowrate is: " + 
				massFlowRate.ToString());




		var dc = new DC("DC", "V1", pressureDropMin, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("Base Pipe Verification: \n");
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>


		//throw new Exception();
	}



    [Fact]
    public void When_PipeFactoryBuildsMockPipe_expectNoException()
    {
		PipeFactory pipeFactory = new PipeFactory("RNL1","out","0");
		Component preCastPipe = pipeFactory.returnPipe("MockPipeCustomResistor");
		// this step is needed to cast the mockPipe as the
		// correct type
		MockPipeCustomResistor mockPipe = (MockPipeCustomResistor)preCastPipe;
		mockPipe.Connect("out","0");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.45, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		this.cout("\n mockPipeClass with pipeFactory \n");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
    }


	// mockpipe class built without factory, reference for
	// factory class
	[Fact]
	public void mockPipeReference_NoFactoryBuild()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
	 MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1");
	 mockPipe.Connect("out","0");
	 mockPipe.Parameters.A = 2.0e3;
	 mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.45, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		this.cout("\n mockPipeReference_NoFactoryBuild \n");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}

	// nonlinear resistor reference
	[Fact]
	public void nonlinearResistorReference_NoFactoryBuild()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
		NonlinearResistor NLR = new NonlinearResistor("RNL1", "out", "0");
		NLR.Parameters.A = 2.0e3;
		NLR.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				NLR
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.45, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		this.cout("\n nonlinearResistorReference_NoFactoryBuild \n");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}

    [Fact]
    public void When_wrongPipeType_ExpectException()
    {
		// setup
		PipeFactory pipeFactory = new PipeFactory("hello123");

		string pipeType = "";

		// now i'm going
		string listOfComponents;
		listOfComponents = pipeFactory.generateList();
		string refErrorMsg;
		refErrorMsg = "";
		refErrorMsg += "\n";
		refErrorMsg += "Your pipeType :" + pipeType + " doesn't exist \n";
		refErrorMsg += "Please consider using pipeTypes \n from the following list";
		refErrorMsg += listOfComponents;

		try{
		pipeFactory.returnPipe(pipeType);
		}
		catch (Exception exception)
		{
			string exceptionMsg;
			exceptionMsg = exception.Message;
			//Console.WriteLine(exceptionMsg);
			//
			Assert.Equal(refErrorMsg,exceptionMsg);
			return;

		}
		Assert.True(1 == 0);
    }


	// this test here is to help us print useful data
	[Fact]
	public void SandBoxPrintResult()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
	 MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1");
	 mockPipe.Connect("out","0");
	 mockPipe.Parameters.A = 2.0e3;
	 mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.45, 1.5, 0.05);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		this.cout("mockPipe without using pipeFactory");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);
		double current = -currentExport.Value;

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}
	
	// this test is to see if operating point experiments can be done 
	[Fact]
	public void When_OperatingPoint_Expect_NoException()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
	 MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1", "out", "0");
	 mockPipe.Parameters.A = 2.0e3;
	 mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 1.5),
				mockPipe
				);

		// Setup the simulation and export our current
		var op = new OP("OP");
		op.Run(ckt);

	}
	
	// this is a test to see if the connect method can be separated from
	// the constructor
	[Fact(Skip = "already tested")]
	public void When_MockPipeCustomResistorConstructorOverLoad_Expect_NoException()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
	 MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1");
	 mockPipe.Connect("out","0");
	 mockPipe.Parameters.A = 2.0e3;
	 mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", -2.0, 2.0, 1e-2);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}

	// this is a test to  see results for nonlinear resistor
	[Fact]
	public void NonlinearResistor_ReferenceResult()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
		this.cout("NonlinearResistor_ReferenceResult");
		NonlinearResistor NLR = new NonlinearResistor("RNL1", "out", "0");
		NLR.Parameters.A = 2.0e3;
		NLR.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				NLR
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.8, 2.0, 0.2);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);

		currentExport.Destroy();

	}

	[Fact]
	public void mockPipeResult()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
		this.cout("MockPipeResult");

		// test for mockpipe
		MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1", "out", "0");
		mockPipe.Connect("out","0");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 


		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", 1.8, 2.0, 0.2);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);

		currentExport.Destroy();


	}


	// this is a test to see if a vanilla nonlinear resistor can work
	[Fact(Skip = "already tested")]
	public void When_MockPipeCustomResistor_Expect_NoException()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
		MockPipeCustomResistor mockPipe = new MockPipeCustomResistor("RNL1", "out", "0");
		mockPipe.Parameters.A = 2.0e3;
		mockPipe.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				mockPipe
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", -2.0, 2.0, 1e-2);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}
	// this is a test to see if a vanilla nonlinear resistor can work

	[Fact(Skip = "already tested")]
	public void When_RunNonlinearResistor_Expect_NoException()
	{
		// <example_customcomponent_nonlinearresistor_test>
		//
		NonlinearResistor NLR = new NonlinearResistor("RNL1", "out", "0");
		NLR.Parameters.A = 2.0e3;
		NLR.Parameters.B = 0.5; 

		// Build the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "out", "0", 0.0),
				NLR
				);

		// Setup the simulation and export our current
		var dc = new DC("DC", "V1", -2.0, 2.0, 1e-2);
		var currentExport = new RealPropertyExport(dc, "V1", "i");
		dc.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			System.Console.Write("{0}, ".FormatString(current));
		};
		dc.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
	}
}
