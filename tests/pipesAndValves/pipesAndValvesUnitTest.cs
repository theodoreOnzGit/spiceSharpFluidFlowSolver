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
	public void When_IsothermalPipeGetDerivedQuantitesExpectNoError(){

		// this step is needed to cast the mockPipe as the
		// correct type
		Component preCasePipe = new IsothermalPipe("isothermalPipe1","out","0");
		IsothermalPipe testPipe = (IsothermalPipe)preCasePipe;
		testPipe.Connect("out","0");
		testPipe.Parameters.A = 2.0e3;
		testPipe.Parameters.B = 0.5; 

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
