# pipesAndValves Test Readme

This is the test suite for the pipes and valve classes.



## Tests to Confirm if MockPipes and CustomResistors perform the same way in the same circuit

The first step is to see if I can build a custom component class 
using nonlinear custom resistors as a template.

This class is functionally similar to nonlinear resistor class
I only changed the name of the class and removed the generated
parameters attribute from BaseParameters.

To help me test if they are functionally similar,
I had to shove both the nonlinear resistor into a circuit, and test
it. Replace it with a mock pipe class and also test it.

Note that i use this.cout to print very often. It is just 
another way of writing console writeline. But it works
for windows OS as well because Console.Writeline
doesn't work in their xunit tests.

```csharp
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
```

I used a DC circuit and swept the voltage of V1 from 1.8 to 2.0 with 
a 0.2V interval. I changed nothing else.

This was the output in the terminal.

```zsh
NonlinearResistor_ReferenceResult
8.1E-07,
```

I accessed the Parameters object directly without using set 
parameters to see if I could refrain from using the generated
parameters attribute.

```csharp

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
```
Then, this was the result.


```zsh
MockPipeResult
8.1E-07
```

This is notably a manual way of testing things. 

With this basic foundation I could now create my own custom
components and know what to do.

### Tests for pipeFactory

The next step was to test for pipeFactory.
PipeFactory Class helps us to instantiate various
implementation of pipe.

However, the problem i found here is that Component
classes do not have Parameter objects embedded within
them.

Hence, i needed to typecase the component objects
as MockPipeCustomResistor Objects before accessing their
Parameters varibles.

```csharp
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
```

The underlying code is in another file. This just shows the test
file.

Here this shows that the factory works. Not that it necessarily produces
the right results.

Here are results produced:

```zsh
mockPipeReference_NoFactoryBuild
5.256249999999999E-07, 5.625E-07,
```

For reference, here is code that builds mockpipe without the factory.

```csharp
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

```

Here are the results for the no factory build:
```zsh
mockPipeReference_NoFactoryBuild
5.256249999999999E-07, 5.625E-07, 

```

To really hit the point home, i'm going to test it with a 
nonlinear resistor (vanilla code).

```csharp
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

```

And here are the results:

```zsh
nonlinearResistorReference_NoFactoryBuild 

5.256249999999999E-07, 5.625E-07
```

Everything checks out!

I used print at this time rather than assert, because
I don't really know how to extract data from simulations yet..
### Tests for Churchill Correlation

The next bit is to start making classes and functions for estimating
friction factor.

I use the [churchill correlation](https://neutrium.net/fluid-flow/pressure-loss-in-pipe/)
as the basis because it is not piecewise like the other correlations.

To test for validity within the laminar flow regime
, i do use the laminar flow equation for friction factor
16/Re.
```csharp

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
```

What this Assert.Equal is saying is that the result error fraction
is equal to 0.0 to one decimal place (or 10%).

The above method, Assert.Equal, allows me to see the reference and 
actual values if it is not equal.

Once this test was successful, i wanted to see how accurate this
equation was.

The results show that it could be accurate to within 4% of the 
16/Re equation in the laminar region.

```csharp

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
```

As a last note, I wanted to show that it was accurate to within 2% for
Re<2000
```csharp

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
```

For the turbulent region i was able to do a similar test

```csharp

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

```

I used the calculator [here](https://www.ajdesigner.com/php_colebrook/colebrook_equation.php#ajscroll) to help
me calculate the friction factor reference values.

This is based off colebrook forumala.






























