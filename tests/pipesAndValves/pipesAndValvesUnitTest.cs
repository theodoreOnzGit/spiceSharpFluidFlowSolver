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

	

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224, 4.0)]
	[InlineData(40000, 0.05, 0.07212405402775,5.0)]
	[InlineData(4e5, 0.05, 0.071608351787938, 10.0)]
	[InlineData(4e6, 0.05,  0.071556444535705, 20.0)]
	[InlineData(4e7, 0.05,  0.071551250389636, 100.0)]
	[InlineData(4e8, 0.05, 0.071550730940769, 1000.0)]
	[InlineData(4e9, 0.05, 0.071550678995539, 65.0)]
	[InlineData(4e3, 0.0, 0.039907014055631, 20.0 )]
	[InlineData(4e7, 0.00005, 0.010627694187016, 35.0)]
	[InlineData(4e6, 0.001, 0.019714092419925, 8.9)]
	[InlineData(4e5, 0.01, 0.038055838413508, 50.0)]
	[InlineData(4e4, 0.03,  0.057933060738478, 1.0e5)]
	public void Test_churchillFrictionFactorShouldGetAccurateReTurbulent(
			double Re,
			double roughnessRatio, 
			double referenceDarcyFrictionFactor,
			double lengthToDiameter){
		// the objective of this test is to test the
		// accuracy of getting Re using the getRe function
		//
		// we have a reference Reynold's number
		//
		// and we need to get a Re using
		// fanning friction factor
		// and roughness Ratio
		//
		// we already have roughness ratio
		// but we need Bejan number and L/D
		//
		// Bejan number would be known in real life.
		// however, in this case, we cannot arbitrarily
		// specify it
		// the only equation that works now
		// is Be = f*Re^2*(4L/D)^3/32.0
		// That means we just specify a L/D ratio
		// and that would specify everything.
		// So I'm going to randomly specify L/D ratios and hope that
		// works
		

		// setup
		//
		double referenceRe = Re;

		IFrictionFactorGetRe testObject;
		testObject = new ChurchHillFrictionFactor();


		double fanningFrictionFactor = 0.25*referenceDarcyFrictionFactor;
		double Be = fanningFrictionFactor*Math.Pow(Re,2.0);
		Be *= Math.Pow(4.0*lengthToDiameter,3);
		Be *= 1.0/32.0;

		// act

		double resultRe;
		resultRe = testObject.getRe(Be,roughnessRatio,lengthToDiameter);

		// Assert (manual test)

		// Assert.Equal(referenceRe, resultRe);

		// Assert (auto test)
		// test if error is within 1% of actual Re
		double errorFraction = Math.Abs(resultRe - referenceRe)/Math.Abs(referenceRe);
		double errorTolerance = 0.01;

		Assert.True(errorFraction < errorTolerance);


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
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	[InlineData(4e5, 0.01, 0.038055838413508)]
	[InlineData(4e4, 0.03,  0.057933060738478)]
	public void Test_churchillFrictionFactorShouldBeAccurate_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchHillFrictionFactor();

		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		
		// Assert
		// Now by default, i can assert to a fixed number of decimal places
		// so comparing 99.98 and 99.99 are about the same to two decimal places
		// However, repeat this tactic with smaller numbers,eg
		// 0.00998 and 0.00999
		// this tactic will fail
		// to normalise everything I will use a normalise decimal place
		// I can take the logarithm base 10 of this number, round up
		// because the log10 of a number will give about the number of decimal 
		// places i need to correct for


		int normaliseDecimalPlace(double reference){

			double normaliseDouble = Math.Log10(reference);
			normaliseDouble = Math.Ceiling(normaliseDouble);
			int normaliseInteger;

			normaliseInteger = (int)normaliseDouble;
			// at this stage, i will get the number of decimal places i need to subtract
			// i want to add the correct number of decimal places,
			// so i will just use a negative sign
			normaliseInteger = -normaliseInteger;

			return normaliseInteger;
		}

		int decimalPlaceTest = 1 + normaliseDecimalPlace(referenceDarcyFactor);


		Assert.Equal(referenceDarcyFactor,resultDarcyFactor,decimalPlaceTest);
	}

	[Theory]
	[InlineData(4000, 0.05, 0.076986834889224)]
	[InlineData(40000, 0.05, 0.072124054027755)]
	[InlineData(4e5, 0.05, 0.071608351787938)]
	[InlineData(4e6, 0.05,  0.071556444535705)]
	[InlineData(4e7, 0.05,  0.071551250389636)]
	[InlineData(4e8, 0.05, 0.071550730940769)]
	[InlineData(4e9, 0.05, 0.071550678995539)]
	[InlineData(4e3, 0.0, 0.039907014055631)]
	[InlineData(4e7, 0.00005, 0.010627694187016)]
	[InlineData(4e6, 0.001, 0.019714092419925)]
	[InlineData(4e5, 0.01, 0.038055838413508)]
	[InlineData(4e4, 0.03,  0.057933060738478)]
	public void Test_churchillFrictionFactorErrorNotMoreThan2Percent_Turbulent(double Re,double roughnessRatio, double referenceFrictionFactor){
		// i'm making the variable explicit so the user can see
		// it's darcy friction factor, no ambiguity here

		// Setup
		double referenceDarcyFactor = referenceFrictionFactor;

		// also the above values are visually inspected with respect to the graph
		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchHillFrictionFactor();

		double errorMax = 0.02;
		// Act

		double resultDarcyFactor =  frictionFactorObj.darcy(Re,roughnessRatio);
		

		double error = Math.Abs(referenceDarcyFactor - resultDarcyFactor)/referenceDarcyFactor;

		// Assert
		//

		Assert.True(error < errorMax);




	}

	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	public void Test_churchillFrictionFactorErrorNotMoreThan2Percent_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		// this test asserts that the error should not be more than 2%

		double referenceFanning = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchHillFrictionFactor();

		double errorMax = 0.02;

		// Act

		double resultFanning = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFanning,resultFanning,4);

		double error;
		error = Math.Abs(resultFanning - referenceFanning)/referenceFanning;
		
		Assert.True(error < errorMax);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
	}

	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
	[InlineData(600, 0.05)]
	[InlineData(800, 0.05)]
	[InlineData(1000, 0.05)]
	[InlineData(1200, 0.05)]
	[InlineData(1400, 0.05)]
	[InlineData(1600, 0.05)]
	[InlineData(1800, 0.05)]
	[InlineData(2000, 0.05)]
	[InlineData(2200, 0.05)]
	public void Test_churchillFrictionFactorErrorNotMoreThan4Percent_Laminar(double Re,double roughnessRatio){
		// this tests the churchill relation against the 
		// laminar flow friction factor
		// fanning is 16/Re
		// and no matter the roughness ratio, I should get the same result
		// however, roughness ratio should not exceed 0.1
		// as maximum roughness ratio in charts is about 0.05
		//
		// Setup

		// this test asserts that the error should not be more than 2%

		double referenceFanning = 16/Re;

		IFrictionFactor frictionFactorObj;
		frictionFactorObj = new ChurchHillFrictionFactor();

		double errorMax = 0.04;

		// Act

		double resultFanning = frictionFactorObj.fanning(Re,roughnessRatio);

		// Assert
		//
		// I want to use a 10 percent difference rather than absolute value
		// Assert.Equal(referenceFanning,resultFanning,4);

		double error;
		error = Math.Abs(resultFanning - referenceFanning)/referenceFanning;
		
		Assert.True(error < errorMax);
		// I have asserted that the churchill friction factor correlation is accurate to 
		// 10% up to Re=2200 with the laminar flow correlation,
		// this is good
	}


	[Theory]
	[InlineData(100, 0.05)]
	[InlineData(200, 0.05)]
	[InlineData(300, 0.05)]
	[InlineData(400, 0.05)]
	[InlineData(400, 0.0)]
	[InlineData(500, 0.05)]
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
