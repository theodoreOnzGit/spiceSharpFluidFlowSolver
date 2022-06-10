using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

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

	// this test will test the churchill correlation over some
	// values using an online colebrook calculator
	// https://www.engineeringtoolbox.com/colebrook-equation-d_1031.html
	// https://www.ajdesigner.com/php_colebrook/colebrook_equation.php#ajscroll
	// the online calculators return a darcy friction factor
	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224)]
	[InlineData(40000, 0.05, 0.072124054027755)]
	[InlineData(4e5, 0.05, 0.071608351787938)]
	[InlineData(4e6, 0.05,  0.071556444535705)]
	[InlineData(4e7, 0.05,  0.071551250389636)]
	[InlineData(4e8, 0.05, 0.071550730940769)]
	[InlineData(4e9, 0.05, 0.071550678995539)]
	public void Test_churchillFrictionFactorShouldBeAccurate_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here
		double referenceDarcyFactor = referenceFrictionFactor;
	}




	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	[InlineData(2200, 0.05)]
	public void Test_churchillFrictionFactorShouldBeAccurate_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		double referenceFrictionFactor = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchHillFrictionFactor();

		// Act

		double resultFrictionFactor = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFrictionFactor,resultFrictionFactor,4);

		double resultErrorFraction;
		resultErrorFraction = Math.Abs(resultFrictionFactor - referenceFrictionFactor)/referenceFrictionFactor;
		
		Assert.Equal(0.0, resultErrorFraction,1);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
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
	//[Fact]
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

	// this is a test to see if a vanilla nonlinear resistor can work
	// this is a test to see if a vanilla nonlinear resistor can work
	//[Fact]
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

	//[Fact]
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