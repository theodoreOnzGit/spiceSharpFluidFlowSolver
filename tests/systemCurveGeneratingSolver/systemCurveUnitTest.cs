using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

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
	}

	[Theory]
	[InlineData(0.0005)]
	[InlineData(0.00005)]
	[InlineData(0.000005)]
	[InlineData(0.0000005)]
	public void whenLinearInterpolationExpectCorrectMassFlow_upToRe100000(double 
			pressureDropValueJoulePerKg){


		IList<double> ReValues = new List<double>();
		IList<double> BeValues = new List<double>();

		// i'll be using these functions repeatedly
		double getBejanFromKinematicPressureDrop(SpecificEnergy 
				kinematicPressureDrop, IsothermalPipe 
				pipe){
			// Be_D = kinPressureDrop * D^2/nu^2
			// D = hydraulicDiameter
			// nu = kinematicViscosity
			double Be_D;

			double nuSqared = Math.Pow(pipe.Parameters.fluidKinViscosity.As( 
						KinematicViscosityUnit.SquareMeterPerSecond)
					,2.0);

			double Dsquared = Math.Pow(pipe.Parameters.hydraulicDiameter.As(
						LengthUnit.Meter)
					,2.0);


			Be_D = kinematicPressureDrop.As(SpecificEnergyUnit.
					JoulePerKilogram) * Dsquared / nuSqared;

			return Be_D;

		}

		MassFlow massFlowrateFromRe(double Re,
				IsothermalPipe pipe){

			// Re = massflow/
			MassFlow flowrate =
				pipe.Parameters.crossSectionalArea()/
				pipe.Parameters.hydraulicDiameter*
				pipe.Parameters.fluidViscosity*
				Re;

			return flowrate.ToUnit(MassFlowUnit.
					KilogramPerSecond);

		}

		// let's start with a Re spacing value of 100
		for (int i = 0; i < 1000; i++)
		{
			double ReSpacing = 100.0;
			double ReValue = ReSpacing * i;
			ReValues.Add(ReValue);

			// now i create a new isothermalPipe
			IsothermalPipe testPipe = new IsothermalPipe(
					"isothermalPipe1","out","0");
			// then i obtain pressureDropValues


			MassFlow flowrate = massFlowrateFromRe(ReValue,
					testPipe);
			
			// once i have the massflowrate, i can then obtain pressureDrops

			SpecificEnergy kinematicPressureDrop =
				testPipe.getKinematicPressureDrop(flowrate).ToUnit(
						SpecificEnergyUnit.JoulePerKilogram);



			// with that i can now get a Bejan Number


			// from this Bejan number let's put this into the BeValues list
			//
			double bejanNumber;
			bejanNumber = getBejanFromKinematicPressureDrop(kinematicPressureDrop,
					testPipe);
			BeValues.Add(bejanNumber);
		}
		// end of for loop

		// now that we finished our data generation,
		// we can then start interpolation
		IInterpolation _linear;
		_linear = Interpolate.Linear(BeValues,ReValues);


		// with this we can make a function to guess massFlowrate 
		// using Pressure Drop
		//
		SpecificEnergy testKinematicPressureDrop = 
			new SpecificEnergy(pressureDropValueJoulePerKg, 
					SpecificEnergyUnit.JoulePerKilogram);
		// i can use this testKinematicPressureDrop to test the value of 
		// mass flowrate using the testPipe


		IsothermalPipe testPipe2 = new IsothermalPipe(
				"isothermalPipe2","out","0");

		MassFlow _referenceMassFlow = testPipe2.getMassFlowRate(
				testKinematicPressureDrop);

		double Be = getBejanFromKinematicPressureDrop(testKinematicPressureDrop,
				testPipe2);
		double Re = _linear.Interpolate(Be);

		// now i've got my Reynold's number, i can get my mass flowrate



		// Act
		MassFlow _resultInterpolatedMassFlow =
			massFlowrateFromRe(Re, testPipe2);

		

		// Assert 
		// I want to check how equal these two results are

		if(Re > 100000){
			throw new Exception("Re is out of interpolation range!");
		}

		Assert.Equal(_referenceMassFlow.As(MassFlowUnit.KilogramPerSecond),
				_resultInterpolatedMassFlow.As(MassFlowUnit.KilogramPerSecond),
				2);

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
