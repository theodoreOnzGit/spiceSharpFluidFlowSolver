using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public class fluidEntityTests : testOutputHelper
{
	public fluidEntityTests(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

	[Fact(Skip = "sandbox")]
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
				MassFlowUnit.KilogramPerSecond);


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
			As(LengthUnit.Meter);
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);

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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.KilogramPerSecond);





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
				MassFlowUnit.KilogramPerSecond);

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
		Assert.Equal(massFlowRate.As(MassFlowUnit.KilogramPerSecond),
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),3);

		//throw new Exception();
	}



	

	// now suppose i have a pressureDrop
	// and i wanted to solve for mass flowrate, i should be able to get it
	// done using MathNet bisection
	// for a 1.45 m^2/s^2 pressureDrop
	// i should get approx 3660 kg/s of flow in my isothermalPipe
	// if i wanted to test for 3 pipes in series
	// i could make the length of each of these pipes
	// 1/3 that of the origianl pipe length (10m)
	//
	
	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	[InlineData(0.1)]
	[InlineData(-0.1)]
	public void WhenKinematicPressureDropSuppliedExpectMassFlowrateValue(
			double kinematicPressureDropVal){


		// Setup 
		// the simulation and export our current
		Component preCastPipe = new IsothermalPipe("isothermalPipe1");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("pumpOutlet","1");
		testPipe.Parameters.pipeLength *= 1.0/3.0;
		preCastPipe = new IsothermalPipe("isothermalPipe2");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("1","2");
		testPipe2.Parameters.pipeLength *= 1.0/3.0;
		preCastPipe = new IsothermalPipe("isothermalPipe3");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("2","0");
		testPipe3.Parameters.pipeLength *= 1.0/3.0;

		// Build the circuit
		SpiceSharp.Entities.IFluidEntityCollection testCkt = new FluidSeriesCircuit(
				new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
				testPipe,
				testPipe2,
				testPipe3
				);

		// now in theory, i should have a pipe 3x as long
		// i shall call this testPipe4
		IsothermalPipe testPipe4 = new IsothermalPipe("isothermalPipe4");
		testPipe4.Connect("pumpOutlet","0");

		SpiceSharp.Entities.IFluidEntityCollection referenceCkt = new FluidSeriesCircuit(
				new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
				testPipe4
				);

		// let's run this reference circuit as usual
		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(referenceCkt);
		double massFlowRateReferenceValue;
		massFlowRateReferenceValue = steadyStateSim.simulationResult;

		// Act
		// try to supply a pressureDrop value here and extract a mass flowrate
		// value
		double massFlowRateResultValue = 0.0;

		MassFlow massFlowRateTestResult =
			testCkt.getMassFlowRate(new SpecificEnergy(kinematicPressureDropVal,
						SpecificEnergyUnit.JoulePerKilogram));

		massFlowRateResultValue = massFlowRateTestResult.As(
				MassFlowUnit.KilogramPerSecond);

		// Assert 

		Assert.Equal(massFlowRateReferenceValue,
				massFlowRateResultValue,3);



	}

	// this is the same test but for dynamic pressure Drop
	//
	

	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	[InlineData(0.1)]
	[InlineData(-0.1)]
	public void WhenDynamicPressureDropSuppliedExpectMassFlowrateValue(
			double kinematicPressureDropVal){


		// Setup 

		// First,
		// the simulation setup
		// 3 pipes in a row with 1/3 length compared to a normal pipe
		Component preCastPipe = new IsothermalPipe("isothermalPipe1");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("pumpOutlet","1");
		testPipe.Parameters.pipeLength *= 1.0/3.0;
		preCastPipe = new IsothermalPipe("isothermalPipe2");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("1","2");
		testPipe2.Parameters.pipeLength *= 1.0/3.0;
		preCastPipe = new IsothermalPipe("isothermalPipe3");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("2","0");
		testPipe3.Parameters.pipeLength *= 1.0/3.0;

		// Next let's get our dynamic pressure drop value,
		// by first getting the density right
		SpecificEnergy referenceKinematicPressure = 
			new SpecificEnergy(kinematicPressureDropVal, 
					SpecificEnergyUnit.JoulePerKilogram);

		// we don't really have density so to speak but
		// we have the ratio of kinematic to dynamic viscosity
		// mu = rho * nu
		// rho = mu/nu

		DynamicViscosity fluidViscosity = 
			testPipe.Parameters.fluidViscosity;

		fluidViscosity = fluidViscosity.ToUnit(DynamicViscosityUnit.
				PascalSecond);

		KinematicViscosity fluidKinViscosity =
			testPipe.Parameters.fluidKinViscosity;

		fluidKinViscosity = fluidKinViscosity.ToUnit(KinematicViscosityUnit.
				SquareMeterPerSecond);

		Density fluidDesnity = fluidViscosity/fluidKinViscosity;
		fluidDesnity = fluidDesnity.ToUnit(
				DensityUnit.KilogramPerCubicMeter);

		
		
		Pressure referencePressure = fluidDesnity * referenceKinematicPressure;
		referencePressure = referencePressure.ToUnit(
				PressureUnit.Pascal);

		// Build the circuit
		SpiceSharp.Entities.IFluidEntityCollection testCkt = new FluidSeriesCircuit(
				new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
				testPipe,
				testPipe2,
				testPipe3
				);

		// now in theory, i should have a pipe 3x as long
		// i shall call this testPipe4
		IsothermalPipe testPipe4 = new IsothermalPipe("isothermalPipe4");
		testPipe4.Connect("pumpOutlet","0");

		SpiceSharp.Entities.IFluidEntityCollection referenceCkt = new FluidSeriesCircuit(
				new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
				testPipe4
				);

		// let's run this reference circuit as usual
		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "V1", "i");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			steadyStateSim.simulationResult = current;
		};
		steadyStateSim.Run(referenceCkt);
		double massFlowRateReferenceValue;
		massFlowRateReferenceValue = steadyStateSim.simulationResult;

		// Act
		// try to supply a pressureDrop value here and extract a mass flowrate
		// value
		double massFlowRateResultValue = 0.0;

		MassFlow massFlowRateTestResult =
			testCkt.getMassFlowRate(referencePressure);

		massFlowRateResultValue = massFlowRateTestResult.As(
				MassFlowUnit.KilogramPerSecond);

		// Assert 

		Assert.Equal(massFlowRateReferenceValue,
				massFlowRateResultValue,3);



	}


	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(-1e-2)]
	[InlineData(1e-2)]
	[InlineData(0.0)]
	public void When_FluidSeriesCircuitPressureDropExpect3xPressureDrop(
			double kinematicPressureDropVal){

		// Setup the simulation and export our current
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
		SpiceSharp.Entities.IFluidEntityCollection ckt = new FluidSeriesCircuit(
				new VoltageSource("V1", "pumpOutlet", "0", kinematicPressureDropVal),
				testPipe,
				testPipe2,
				testPipe3
				);

		// first we get the Bejan number
		// for a pipe which is 3x as long
		//
		// so this means that my pipeLength is 3x as long
		// Be_L is about 3x as long in other words
		//
		MassFlow returnMassFlowRateValue(double kinematicPressureDropValJoulePerKg){

			double Be;
			Be = kinematicPressureDropValJoulePerKg;
			Be *= testPipe.Parameters.pipeLength.
				As(LengthUnit.Meter)*3.0;
			Be *= testPipe.Parameters.pipeLength.
				As(LengthUnit.Meter)*3.0;
			Be /= testPipe.Parameters.fluidKinViscosity.
				As(KinematicViscosityUnit.SquareMeterPerSecond);
			Be /= testPipe.Parameters.fluidKinViscosity.
				As(KinematicViscosityUnit.SquareMeterPerSecond);

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

			return massFlowRate.ToUnit(MassFlowUnit.KilogramPerSecond);
		}

		MassFlow massFlowRate =
			returnMassFlowRateValue(kinematicPressureDropVal);

		// Act
		// now if i feed in this massFlowrate, i should get
		// the pressureDrop as before

		SpecificEnergy kinematicPressureDropResult;
		kinematicPressureDropResult = ckt.getKinematicPressureDrop(
				massFlowRate);

		double kinematicPressureDropResultVal
			= kinematicPressureDropResult.As(
					SpecificEnergyUnit.JoulePerKilogram);

		// Assert
		Assert.Equal(kinematicPressureDropVal,
				kinematicPressureDropResultVal,3);

	}

	// here we test if the FluidParallelSubCircuit
	// will function with the fluidSeriesCircuit in giving us the correct 
	// flowrate
	
	[Theory]
	[InlineData(1.45)]
	[InlineData(0.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_FluidSeriesCircuitwithParallelSubCkt_getMassFlowrate_expectCorrectFlow(
			double pressureDrop){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate
		//
		// this basically ensures that FluidParalleSubCircuit functions
		// like subcircuit if given the same parameters

		// this step is needed to cast the testPipe as the
		// correct type
		//
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("parallelIn","parallelOut");

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");


		// Build the circuit
		SpiceSharp.Entities.IFluidEntityCollection ckt = 
			new FluidSeriesCircuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				fluidSubCkt
				);

		// Setup the simulation and export our current


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
		//
		double massFlowRateReferenceValueKgPerS;
		massFlowRateReferenceValueKgPerS = steadyStateSim.simulationResult;

		MassFlow massFlowRateTestResult;
		massFlowRateTestResult = ckt.getMassFlowRate(
				new SpecificEnergy(pressureDrop,
					SpecificEnergyUnit.JoulePerKilogram));

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
		Assert.Equal(massFlowRateReferenceValueKgPerS,
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),3);

		//throw new Exception();
	}


	// here we will test if the FluidParallelSubCircuit
	// will function by itself in giving us the correct
	// flowrate
	//
	[Theory]
	[InlineData(1.45)]
	[InlineData(0.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_FluidParallelSubcircuit_getMassFlowrate_expectCorrectFlow(
			double pressureDrop){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate
		//
		// this basically ensures that FluidParalleSubCircuit functions
		// like subcircuit if given the same parameters

		// this step is needed to cast the testPipe as the
		// correct type
		//
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("parallelIn","parallelOut");

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");


		// Build the circuit
		SpiceSharp.Entities.IFluidEntityCollection ckt = 
			new FluidSeriesCircuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				fluidSubCkt
				);

		// Setup the simulation and export our current


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
		//
		double massFlowRateReferenceValueKgPerS;
		massFlowRateReferenceValueKgPerS = steadyStateSim.simulationResult;

		MassFlow massFlowRateTestResult;
		massFlowRateTestResult = fluidSubCkt.getMassFlowRate(
				new SpecificEnergy(pressureDrop,
					SpecificEnergyUnit.JoulePerKilogram));

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
		Assert.Equal(massFlowRateReferenceValueKgPerS,
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),3);

		//throw new Exception();
	}
	// here we test fluidParallelCircuit
	
	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_FluidSeriesCircuitParallelSubcircuitSetupExpect3xFlow(
			double pressureDrop){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate
		//
		// this basically ensures that FluidParalleSubCircuit functions
		// like subcircuit if given the same parameters

		// this step is needed to cast the testPipe as the
		// correct type
		//
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Connect("parallelIn","parallelOut");
		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Connect("parallelIn","parallelOut");

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");


		// Build the circuit
		SpiceSharp.Entities.IFluidEntityCollection ckt = 
			new FluidSeriesCircuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				new FluidParallelSubCircuit("X1", subckt).Connect("out" , "0")
				);

		// Setup the simulation and export our current

		double Be;
		Be = pressureDrop;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter);
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);

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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.KilogramPerSecond);


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
				MassFlowUnit.KilogramPerSecond);

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
		Assert.Equal(massFlowRate.As(MassFlowUnit.KilogramPerSecond),
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),3);

		//throw new Exception();
	}


	[Fact(Skip = "not mean to be tested, reference code")]
	public void subcircuitReferenceCode(){
		// Define the subcircuit
		var subckt = new SubcircuitDefinition(new Circuit(
					new Resistor("R1", "a", "b", 1e3),
					new Resistor("R2", "b", "0", 1e3)),
				"a", "b");

		// Define the circuit
		var ckt = new Circuit(
				new VoltageSource("V1", "in", "0", 5.0),
				new Subcircuit("X1", subckt).Connect("in", "out"));

	}

	// here we test if our FluidSeriesCircuit
	// is able to behave like a normal circuit
	// so far so good

	[Theory]
	[InlineData(1.45)]
	[InlineData(-1.45)]
	[InlineData(0.0)]
	public void When_FluidSeriesCircuitParallelSetupExpect3xFlow(
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
		SpiceSharp.Entities.IFluidEntityCollection ckt = 
			new FluidSeriesCircuit(
				new VoltageSource("V1", "out", "0", pressureDrop),
				testPipe,
				testPipe2,
				testPipe3
				);

		// Setup the simulation and export our current

		double Be;
		Be = pressureDrop;
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter);
		Be *= testPipe.Parameters.pipeLength.
			As(LengthUnit.Meter);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);
		Be /= testPipe.Parameters.fluidKinViscosity.
			As(KinematicViscosityUnit.SquareMeterPerSecond);

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

		massFlowRate = massFlowRate.ToUnit(MassFlowUnit.KilogramPerSecond);


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
				MassFlowUnit.KilogramPerSecond);

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
		Assert.Equal(massFlowRate.As(MassFlowUnit.KilogramPerSecond),
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),3);

		//throw new Exception();
	}
	// Tests to do with fluid entity,
	//
	// (1) Does the sum of pressure drop from one
	// fluid entity equal that of the code
	// outside the entity?
	//
	// (2) if we have just one fluid entity within a 
	// fluid entity collection, does that equal the 
	// pressure drop from code outside the entity?
	//

	[Theory]
	[InlineData(0.45)]
	[InlineData(150)]
	[InlineData(3660)]
	public void WhenSingleFluidEntity_ShouldEqualPressureDropOfOneComponent(
			double massFlowValueKgPerS){
	
		// Setup
		// First we set up our objects first to see if
		// And we cast them to IFluidEntity
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		SpiceSharp.Entities.IFluidEntity testPipe 
			= (SpiceSharp.Entities.IFluidEntity)preCastPipe;

		IsothermalPipe referencePipe;
		referencePipe = (IsothermalPipe)preCastPipe;

		// Secondly we also bring in our standard friction
		// factor objects
		// so that we can calculate a pressuredrop
		// given a mass flowrate

		double getKinematicPressureDrop(double massFlowValueKgPerS){
			// first we get a mass flowrate value in kg/s
			// and then we try to get the Reynold's number first
			MassFlow flowrate = new MassFlow(massFlowValueKgPerS,
					MassFlowUnit.KilogramPerSecond);

			Area XSArea = referencePipe.Parameters.crossSectionalArea();

			DynamicViscosity mu = referencePipe.Parameters.
				fluidViscosity;

			KinematicViscosity nu = referencePipe.Parameters.
				fluidKinViscosity;

			Length hydraulicDiameter = referencePipe.Parameters.
				hydraulicDiameter;

			// Re = rho U D/ mu = massflow/XSArea * D/mu

			double Re = flowrate.As(MassFlowUnit.KilogramPerSecond);
			Re /= XSArea.As(AreaUnit.SquareMeter);
			Re *= hydraulicDiameter.As(LengthUnit.Meter);
			Re /= mu.As(DynamicViscosityUnit.PascalSecond);

			// we can then get the Bejan number
			// or we can just get the friction factor first

			double darcyFrictionFactor;
			double roughnessRatio = referencePipe.Parameters.roughnessRatio();
			double lengthToDiameter = referencePipe.Parameters.lengthToDiameter();

			IFrictionFactorJacobian _jacobianObject;
			_jacobianObject = new StabilisedChurchillJacobian();

			darcyFrictionFactor = _jacobianObject.darcy(
					Re, roughnessRatio);

			// we calculate fLDK after that
			// assuming there is no inner resistance whatsoever
			double fLDKReSq;
			fLDKReSq = darcyFrictionFactor * 
				Math.Pow(Re,2.0) *
				lengthToDiameter;

			double Be = fLDKReSq/2.0;

			// Be_D = deltaP * D^2/nu^2
			// deltaP = Be_D * nu^2/D^2
			SpecificEnergy pressureDrop = nu.Pow(2)/
				hydraulicDiameter.Pow(2);
			pressureDrop *= Be;
			



			return pressureDrop.As(SpecificEnergyUnit.JoulePerKilogram);
		}

		double referencePressreDropJoulePerKg =
			getKinematicPressureDrop(massFlowValueKgPerS);

		MassFlow massFlowrate;
		massFlowrate = new MassFlow(massFlowValueKgPerS,
				MassFlowUnit.KilogramPerSecond);

		// Act
		//
		SpecificEnergy kinematicPressureDropResult =
			testPipe.getKinematicPressureDrop(massFlowrate);

		double kinematicPressureDropResultVal = 
			kinematicPressureDropResult.As(SpecificEnergyUnit.
					JoulePerKilogram);

		// Assert
		//
		//
		Assert.Equal(referencePressreDropJoulePerKg,
				kinematicPressureDropResultVal,2);

	}
	
	[Theory]
	[InlineData(0.45)]
	[InlineData(150)]
	[InlineData(3660)]
	public void WhenSingleFluidEntityCollection_ShouldEqualPressureDropOfOneComponent(
			double massFlowValueKgPerS){
	
		// Setup
		// First we set up our objects first to see if
		// And we cast them to IFluidEntity
		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		SpiceSharp.Entities.IFluidEntity testPipe 
			= (SpiceSharp.Entities.IFluidEntity)preCastPipe;

		IsothermalPipe referencePipe;
		referencePipe = (IsothermalPipe)preCastPipe;
		// now we put this fluid entity into a fluidCircuit

		SpiceSharp.Entities.IFluidEntityCollection ckt = new FluidSeriesCircuit(
				new CurrentSource("I1", "out", "0", massFlowValueKgPerS),
				testPipe
				);
		// Secondly we also bring in our standard friction
		// factor objects
		// so that we can calculate a pressuredrop
		// given a mass flowrate
		//

		double getKinematicPressureDrop(double massFlowValueKgPerS){
			// first we get a mass flowrate value in kg/s
			// and then we try to get the Reynold's number first
			MassFlow flowrate = new MassFlow(massFlowValueKgPerS,
					MassFlowUnit.KilogramPerSecond);

			Area XSArea = referencePipe.Parameters.crossSectionalArea();

			DynamicViscosity mu = referencePipe.Parameters.
				fluidViscosity;

			KinematicViscosity nu = referencePipe.Parameters.
				fluidKinViscosity;

			Length hydraulicDiameter = referencePipe.Parameters.
				hydraulicDiameter;

			// Re = rho U D/ mu = massflow/XSArea * D/mu

			double Re = flowrate.As(MassFlowUnit.KilogramPerSecond);
			Re /= XSArea.As(AreaUnit.SquareMeter);
			Re *= hydraulicDiameter.As(LengthUnit.Meter);
			Re /= mu.As(DynamicViscosityUnit.PascalSecond);

			// we can then get the Bejan number
			// or we can just get the friction factor first

			double darcyFrictionFactor;
			double roughnessRatio = referencePipe.Parameters.roughnessRatio();
			double lengthToDiameter = referencePipe.Parameters.lengthToDiameter();

			IFrictionFactorJacobian _jacobianObject;
			_jacobianObject = new StabilisedChurchillJacobian();

			darcyFrictionFactor = _jacobianObject.darcy(
					Re, roughnessRatio);

			// we calculate fLDK after that
			// assuming there is no inner resistance whatsoever
			double fLDKReSq;
			fLDKReSq = darcyFrictionFactor * 
				Math.Pow(Re,2.0) *
				lengthToDiameter;

			double Be = fLDKReSq/2.0;

			// Be_D = deltaP * D^2/nu^2
			// deltaP = Be_D * nu^2/D^2
			SpecificEnergy pressureDrop = nu.Pow(2)/
				hydraulicDiameter.Pow(2);
			pressureDrop *= Be;
			



			return pressureDrop.As(SpecificEnergyUnit.JoulePerKilogram);
		}

		double referencePressreDropJoulePerKg =
			getKinematicPressureDrop(massFlowValueKgPerS);

		MassFlow massFlowrate;
		massFlowrate = new MassFlow(massFlowValueKgPerS,
				MassFlowUnit.KilogramPerSecond);

		// Act
		//
		SpecificEnergy kinematicPressureDropResult =
			ckt.getKinematicPressureDrop(massFlowrate);

		double kinematicPressureDropResultVal = 
			kinematicPressureDropResult.As(SpecificEnergyUnit.
					JoulePerKilogram);

		// Assert
		//
		//
		Assert.Equal(referencePressreDropJoulePerKg,
				kinematicPressureDropResultVal,8);

	}
	

	// in this test i want to ensure that FluidEntities can deal with
	// hydrostatic pressure gradients
	[Theory]
	[InlineData(1.45, 0.846910353)]
	[InlineData(0.0, -0.846910353)]
	[InlineData(1.45, 180-0.846910353)]
	[InlineData(0.0, 180+0.846910353)]
	public void WhenFluidEntityInclinedToZeroPressureDrop_ExpectNoFlow(
			double pressureDropValueJoulePerKg,
			double inclineAngleDegrees){

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
				new VoltageSource("V1", "out", "0", pressureDropValueJoulePerKg),
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

		steadyStateSim.Run(ckt);
		double expectedFlowrateKgPerSecond = steadyStateSim.simulationResult;
		
		// now let me prepare the results
		double actualMassFlowRateValueKgPerSecond;

		SpecificEnergy pressureDrop = 
			new SpecificEnergy(pressureDropValueJoulePerKg,
					SpecificEnergyUnit.JoulePerKilogram);
		// Act

		MassFlow actualFlowRate = 
			testPipe.getMassFlowRate(pressureDrop);
		actualMassFlowRateValueKgPerSecond =
			actualFlowRate.As(MassFlowUnit.KilogramPerSecond);

		// Assert
		
		Assert.Equal(expectedFlowrateKgPerSecond, 
				actualMassFlowRateValueKgPerSecond,2);
		// test failed as of 27 jul 2022 1800 hrs
	}
}
