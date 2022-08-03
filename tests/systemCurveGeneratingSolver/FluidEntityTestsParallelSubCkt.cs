using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public partial class fluidEntityTests : testOutputHelper
{
	// here we test if the FluidParallelSubCircuit
	// will function with the fluidSeriesCircuit in giving us the correct 
	// flowrate
	
	//[Theory(Skip = "temporary skip for fast debug")]
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

	// [Theory]
	[Theory(Skip = "expedited testing, temporary skip")]
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
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),2);

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
				massFlowRateTestResult.As(MassFlowUnit.KilogramPerSecond),2);

		//throw new Exception();
	}

	[Theory]
	[InlineData(0.5,0.1,0.3)]
	[InlineData(0.4,0.2,0.3)]
	public void WhenParallelSubCkt_getHydraulicDiameter_expectEnsembleAverage(
			double diameter1,
			double diameter2,
			double diameter3){

		// Setup
		//
		// First let's get the lengths

		Length hydraulicDiameter1 = new Length(diameter1, LengthUnit.Meter);
		Length hydraulicDiameter2 = new Length(diameter2, LengthUnit.Meter);
		Length hydraulicDiameter3 = new Length(diameter3, LengthUnit.Meter);

		Length expectedAverageHydraulicDiameter =
			(hydraulicDiameter1 + hydraulicDiameter2 + hydraulicDiameter3)/3.0;

		// next let's setup the pipes

		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Parameters.hydraulicDiameter = hydraulicDiameter1;
		testPipe.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Parameters.hydraulicDiameter = hydraulicDiameter2;
		testPipe2.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Parameters.hydraulicDiameter = hydraulicDiameter3;
		testPipe3.Connect("parallelIn","parallelOut");

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");
		
		//Act
		//
		Length _actualAverageHydraulicDiameter = 
			fluidSubCkt.getHydraulicDiameter();

		// Assert

		Assert.Equal(expectedAverageHydraulicDiameter.As(LengthUnit.SI),
				_actualAverageHydraulicDiameter.As(LengthUnit.SI),
				4);

	}

	[Theory]
	[InlineData(0.5,0.1,0.3)]
	[InlineData(0.4,0.2,0.3)]
	public void WhenParallelSubCkt_getKinematicViscosity_expectEnsembleAverage(
			double vis1,
			double vis2,
			double vis3){

		// Setup
		//
		// First let's get the lengths

		KinematicViscosity kinViscosity1 = 
			new KinematicViscosity(vis1, 
					KinematicViscosityUnit.SquareMeterPerSecond);
		KinematicViscosity kinViscosity2 = 
			new KinematicViscosity(vis2, 
					KinematicViscosityUnit.SquareMeterPerSecond);
		KinematicViscosity kinViscosity3 = 
			new KinematicViscosity(vis3, 
					KinematicViscosityUnit.SquareMeterPerSecond);

		KinematicViscosity expectedAverageKinematicViscosity =
			(kinViscosity1 + kinViscosity2 + kinViscosity3)/3.0;

		// next let's setup the pipes

		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Parameters.fluidKinViscosity = kinViscosity1;
		testPipe.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Parameters.fluidKinViscosity = kinViscosity2;
		testPipe2.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Parameters.fluidKinViscosity = kinViscosity3;
		testPipe3.Connect("parallelIn","parallelOut");

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");
		
		//Act
		//
		KinematicViscosity _actualAverageFluidKinematicViscosity = 
			fluidSubCkt.getFluidKinematicViscosity();

		// Assert

		Assert.Equal(expectedAverageKinematicViscosity.As(KinematicViscosityUnit.SI),
				_actualAverageFluidKinematicViscosity.As(KinematicViscosityUnit.SI),
				4);

	}

	[Theory]
	[InlineData(0.5,0.1,0.3)]
	[InlineData(0.4,0.2,0.3)]
	public void WhenParallelSubCkt_getArea_expectSum(
			double diameter1,
			double diameter2,
			double diameter3){


		// Setup
		//
		// First let's get the lengths

		Length hydraulicDiameter1 = new Length(diameter1, LengthUnit.Meter);
		Length hydraulicDiameter2 = new Length(diameter2, LengthUnit.Meter);
		Length hydraulicDiameter3 = new Length(diameter3, LengthUnit.Meter);


		// next let's setup the pipes

		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Parameters.hydraulicDiameter = hydraulicDiameter1;
		testPipe.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Parameters.hydraulicDiameter = hydraulicDiameter2;
		testPipe2.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Parameters.hydraulicDiameter = hydraulicDiameter3;
		testPipe3.Connect("parallelIn","parallelOut");

		// let me get my total area from these pipes
		Area expectedTotalArea =
			(testPipe.getXSArea() + 
			 testPipe2.getXSArea() + 
			 testPipe3.getXSArea());

		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");
		
		//Act
		//
		Area _actualArea = 
			fluidSubCkt.getXSArea();

		// Assert

		Assert.Equal(expectedTotalArea.As(AreaUnit.SI),
				_actualArea.As(AreaUnit.SI),
				4);

	}


	[Theory]
	[InlineData(0.5,0.1,0.3, 1.5,1.0,0.5)]
	[InlineData(0.4,0.2,0.3, 0.9,0.88,1.3)]
	public void WhenParallelSubCkt_getDensity_expectAreaWeightedSum(
			double diameter1,
			double diameter2,
			double diameter3,
			double kinVis1,
			double kinVis2,
			double kinVis3){


		// Setup
		//
		// First let's get the lengths and kinematic Viscosities
		//
		// Because my testpipe obtains density by finding ratios of 
		// kinematicViscosity to dynamic Viscosity
		// i will just randomly switch up the kinematic viscosity up

		Length hydraulicDiameter1 = new Length(diameter1, LengthUnit.Meter);
		Length hydraulicDiameter2 = new Length(diameter2, LengthUnit.Meter);
		Length hydraulicDiameter3 = new Length(diameter3, LengthUnit.Meter);

		KinematicViscosity kinViscosity1 = 
			new KinematicViscosity(kinVis1, 
					KinematicViscosityUnit.Centistokes);
		KinematicViscosity kinViscosity2 = 
			new KinematicViscosity(kinVis2, 
					KinematicViscosityUnit.Centistokes);
		KinematicViscosity kinViscosity3 = 
			new KinematicViscosity(kinVis3, 
					KinematicViscosityUnit.Centistokes);


		// next let's setup the pipes

		Component preCastPipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCastPipe;
		testPipe.Parameters.hydraulicDiameter = hydraulicDiameter1;
		testPipe.Parameters.fluidKinViscosity = kinViscosity1;
		testPipe.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe2","out","0");
		IsothermalPipe testPipe2 = (IsothermalPipe)preCastPipe;
		testPipe2.Parameters.hydraulicDiameter = hydraulicDiameter2;
		testPipe2.Parameters.fluidKinViscosity = kinViscosity2;
		testPipe2.Connect("parallelIn","parallelOut");

		preCastPipe = new IsothermalPipe("isothermalPipe3","out","0");
		IsothermalPipe testPipe3 = (IsothermalPipe)preCastPipe;
		testPipe3.Parameters.hydraulicDiameter = hydraulicDiameter3;
		testPipe3.Parameters.fluidKinViscosity = kinViscosity3;
		testPipe3.Connect("parallelIn","parallelOut");

		// let me get my total area from these pipes
		Area expectedTotalArea =
			(testPipe.getXSArea() + 
			 testPipe2.getXSArea() + 
			 testPipe3.getXSArea());

		// the above area will help me weight my densities
		// now let's get the expected density
		// were
		// sqrt(rho_avg) = 1/totalArea * (area1*sqrt(rho1) +
		// area2*sqrt(rho2) + area3*sqrt(rho3))

		Density _expectedAverageDensity;

		{
			double sqrtDensity;
			sqrtDensity = 
				testPipe.getXSArea().As(AreaUnit.SI) *
				Math.Pow(testPipe.getFluidDensity().As(DensityUnit.SI),0.5) +
				testPipe2.getXSArea().As(AreaUnit.SI) *
				Math.Pow(testPipe2.getFluidDensity().As(DensityUnit.SI),0.5) +
				testPipe3.getXSArea().As(AreaUnit.SI) *
				Math.Pow(testPipe3.getFluidDensity().As(DensityUnit.SI),0.5);

			sqrtDensity /= expectedTotalArea.As(AreaUnit.SI);
			double densityValue = 
				Math.Pow(sqrtDensity,2.0);

			_expectedAverageDensity = new Density(
					densityValue, DensityUnit.KilogramPerCubicMeter);
		}



		var subckt = new SubcircuitDefinition(new Circuit(
					testPipe,
					testPipe2,
					testPipe3),
				"parallelIn", "parallelOut");

		FluidParallelSubCircuit fluidSubCkt
			= new FluidParallelSubCircuit("X1", subckt);
		fluidSubCkt.Connect("out" , "0");
		
		//Act
		//
		Density _actualAverageDensity = 
			fluidSubCkt.getFluidDensity();
	

		// Assert

		Assert.Equal(_expectedAverageDensity.As(DensityUnit.SI),
				_actualAverageDensity.As(DensityUnit.SI),1
				);

	}
	
	[Theory]
	[InlineData(10980)]
	[InlineData(-10980)]
	[InlineData(0.0)]
	public void When_ParallelSubCkt_getPressureDrop_expectCorrectPressure(
			double massFlowrateValue){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate
		// i'm using a current source this time to see if the solver
		// can handle it
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



		SpecificEnergy expectedPressureDropObj = 
			fluidSubCkt.getKinematicPressureDropUsingIteration(
					new MassFlow(massFlowrateValue, MassFlowUnit.KilogramPerSecond));
		double expectedPressureDrop = expectedPressureDropObj.As(
				SpecificEnergyUnit.JoulePerKilogram);
		// Act

		SpecificEnergy resultPressureDropObj = 
			fluidSubCkt.getKinematicPressureDrop(
					new MassFlow(massFlowrateValue, MassFlowUnit.KilogramPerSecond));
		double resultPressureDrop = resultPressureDropObj.As(
				SpecificEnergyUnit.JoulePerKilogram);


		Assert.Equal(expectedPressureDrop, resultPressureDrop,
				2);
	}

	[Theory(Skip = "numerical instability")]
	[InlineData(10980)]
	[InlineData(-10980)]
	[InlineData(0.0)]
	public void When_ParallelCktISRC_getPressureDrop_expectCorrectPressure(
			double massFlowrateValue){

		// for this test i'm going to have 3 pipes in parallel with the
		// same pressure drop across all of them (no pump curve here
		// or anything)
		// for the same pressure drop across all three pipes, i
		// should expect 3x the mass flowrate
		// i'm using a current source this time to see if the solver
		// can handle it
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
				new CurrentSource("I1", "out", "0", massFlowrateValue),
				fluidSubCkt
				);

		// Setup the simulation and export our current


		ISteadyStateFlowSimulation steadyStateSim = 
			new PrototypeSteadyStateFlowSimulation(
				"PrototypeSteadyStateFlowSimulation");
		var currentExport = new RealPropertyExport(steadyStateSim, "I1", "i");
		var pumpOutletKinPressure = new RealVoltageExport(steadyStateSim,
				"out");
		var freesurfaceKinPressure = new RealVoltageExport(steadyStateSim,
				"0");
		steadyStateSim.ExportSimulationData += (sender, args) =>
		{
			var current = -currentExport.Value;
			var pressreDropValue = pumpOutletKinPressure.Value -
				freesurfaceKinPressure.Value;
			steadyStateSim.simulationResult = pressreDropValue;
		};
		steadyStateSim.Run(ckt);

		currentExport.Destroy();
		// </example_customcomponent_nonlinearresistor_test>
		// the above test also fails
		// numerical instability...

		SpecificEnergy expectedPressureDropObj = 
			fluidSubCkt.getKinematicPressureDrop(
					new MassFlow(massFlowrateValue, MassFlowUnit.KilogramPerSecond));

		double expectedPressureDrop = expectedPressureDropObj.As(
				SpecificEnergyUnit.JoulePerKilogram);

		Assert.Equal(expectedPressureDrop, steadyStateSim.simulationResult,
				2);
	}



	// basically this test sees when i elevate parallel branches unevenly
	// I should expect an exception thrown 
	[Theory]
	[InlineData()]
	public void WhenParallelSubCktElevatedUnevenly_ExpectParallelSubCktException(){
		throw new NotImplementedException();
	}

	[Theory]
	[InlineData()]
	public void WhenParallelSubCktElevated_ExpectCorrectMassFlowrate(){
		throw new NotImplementedException();
	}
	
	[Theory]
	[InlineData()]
	public void WhenParallelSubCktElevatedChgTemp_ExpectCorrectMassFlow(){
		throw new NotImplementedException();
	}


}
