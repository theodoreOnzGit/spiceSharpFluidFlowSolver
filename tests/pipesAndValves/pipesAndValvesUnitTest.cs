using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

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
    public void Test1()
    {
		this.cout("hello there");
    }
	
	// this is a test to see if a vanilla nonlinear resistor can work
	[Fact]
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

	[Fact]
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
