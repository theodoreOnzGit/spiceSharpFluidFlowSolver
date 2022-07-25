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


	[Theory(Skip = "pending other unit tests")]
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

		double Be;
		Be = kinematicPressureDropVal;
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

		// Act
		// now if i feed in this massFlowrate, i should get
		// the pressureDrop as before

		SpecificEnergy kinematicPressureDropResult;
		kinematicPressureDropResult = ckt.getKinematicPressureDrop(
				massFlowRate);

		double kinematicPressureDropResultVal
			= kinematicPressureDropResult.As(
					SpecificEnergyUnit.SI);

		// Assert
		Assert.Equal(kinematicPressureDropVal,
				kinematicPressureDropResultVal);

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

	[Fact]
	public void WhenSingleFluidEntity_ShouldEqualPressureDropOfOneComponent(){
	
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

		double massFlowValueKgPerS = 0.45;
		double referencePressreDropJoulePerKg =
			getKinematicPressureDrop(massFlowValueKgPerS);

		MassFlow massFlowrate;
		massFlowrate = new MassFlow(massFlowValueKgPerS,
				MassFlowUnit.SI);

		// Act
		//
		SpecificEnergy kinematicPressureDropResult =
			testPipe.getKinematicPressureDrop(massFlowrate);

		double kinematicPressureDropResultVal = 
			kinematicPressureDropResult.As(SpecificEnergyUnit.SI);

		// Assert
		//
		//
		Assert.Equal(referencePressreDropJoulePerKg,
				kinematicPressureDropResultVal,6);

	}
}
