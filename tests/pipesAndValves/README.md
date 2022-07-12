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

## Test for Filonenko Friction Factor

Now that the churchill correlation is tested,
the next thing to test is the derivative of the friction factor.

Unfortunately, attempting to test the analytical derivative of
the churchill friction factor against the numerical one proved to
be troublesome in debugging. 

I thus needed an expression that was reasonably accurate yet
almost trivial to differentiate analytically.
[The Filinenko friction factor was found from this paper
.](https://iwaponline.com/ws/article/20/4/1321/73330/Approximations-of-the-Darcy-Weisbach-friction)

I could then use this as a benchmark to test against the
numerical solution.

The steps to test are as follows:

1. Test the filonenko friction factor against colebrook data
2. Test the numerical derivative of the filonenko friction factor
   against the analytical deriviative
3. use the analytical derivative of the filonenko friction factor
   to test the churchill numerical derivative.


$$\frac{1}{\sqrt{f_{Darcy}}}=-2 \log_{10} (\frac{6.6069}{Re^{0.91}}
+ \frac{\varepsilon/D}{3.71})$$

This provides more of a sanity check than anything else.


$$\frac{d}{dRe}\frac{1}{\sqrt{f_{Darcy}}}= -2 \frac{1}{\ln 10}
\frac{6.6069*(-0.91)*Re^{-1.91}}{\frac{6.6069}{Re^{0.91}}
+\frac{\varepsilon/D}{3.71}}$$

$$\frac{d f_{Darcy}}{dRe} = -2 f_{Darcy}^{1.5}
\frac{d}{dRe}\frac{1}{\sqrt{f_{Darcy}}}$$


Now i tested this analytical derivative against the numerical
derivative calculated by Mathnet. They are pretty much equal
to within 0.01%.

This means to say two things:
1. Filonenko Friction factor can now be used as a sanity check
2. The analytical derivative is internally consistent with
   the numerical derivative

So this means that the MathNet thing is working quite well.

## Tests for Validity of the BasePipe class

So far, here are the test results of the basepipe class

```csharp

var dc = new DC("DC", "V1", 1.45, 1.5, 0.05);
var currentExport = new RealPropertyExport(dc, "V1", "i");
this.cout("\n BasePipe without pipeFactory \n");
dc.ExportSimulationData += (sender, args) =>
{
	var current = -currentExport.Value;
	System.Console.Write("{0}, ".FormatString(current));
};
dc.Run(ckt);
double current = -currentExport.Value;

currentExport.Destroy();
// </example_customcomponent_nonlinearresistor_test>
```

Here i am essentially putting in a kinematic pressure of
1.45 $m^2/s^2$ and 1.5 $m^2/s^2$.

Currently, results are as follows:

```csharp
Base Pipe Verification: 
3659.999407477403, 
Base Pipe Verification: 
3723.3071369684676, 
```
This means that we have a flowrate of about 3660 kg/s and 3723 kg/s.

Now, the thing is results are printed
and therefore i have no way of 
estimating programatically testing.

And i have to visually inspect one
by one.

What i can do though is to say the
expected Re and flowrate.

In either case, we still need some code to calculate our reference
mass flowrate value.

We are given kinematic pressure $\Delta p$. And i need a correct
mass flowrate.

For this, I want to get a reference Re. And to get reference Re
we need Be. 

$$ Be = \frac{\Delta p L^2}{\nu^2}$$

```csharp
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
```

From this i can get Re:

```csharp

double Re;
ChurchillFrictionFactorJacobian _jacobianObject;
_jacobianObject = new ChurchillFrictionFactorJacobian();
double roughnessRatio = mockPipe.Parameters.roughnessRatio();
double lengthToDiameter = mockPipe.Parameters.lengthToDiameter();
Re = _jacobianObject.getRe(Be,roughnessRatio,lengthToDiameter);
```

And then convert Re to MassFlowrate in correct units:


```csharp
MassFlow massFlowRate;
	massFlowRate = mockPipe.Parameters.fluidViscosity*
mockPipe.Parameters.crossSectionalArea()/
	mockPipe.Parameters.hydraulicDiameter*
	Re;

	massFlowRate = massFlowRate.ToUnit(MassFlowUnit.SI);

	this.cout("\n The reference Mass flowrate is: " + 
			massFlowRate.ToString());
```

The results are for now, quite satisfactory:

```zsh

 The reference Mass flowrate is: 3660 kg/s
Base Pipe Verification: 
3659.999407477403, Base Pipe Verification: 
3723.3071369684676, 
 nonlinearResistorReference_NoFactoryBuild 
```

The latter answer is for a kinematic pressure of
1.5 $m^2/s^2$. Whereas the minimum kinematic pressure is
1.45 $m^2/s^2$.

So far this shows that pipe flow in this range is definitely
correct as expected!

## Going Beyond BasePipe

Now that the BasePipe is confirmed to at least known to function 
correctly compraed to the base classes which calculate Re and all 
associated functions. We now want to move on to make the BasePipe
class suitable for use in a transient simulation.

Only thereafter we want to use a OPCUA server to help us perform
transient simulation. 

Now to start off, let's see how we can get a good steady state simulation.

Here are a list of things to do:

1. ensure the pipe can handle zero and reverse flow
2. ensure that we are using only steady state simulations rather than 
DC simulations
3. ensure that we are able to pass out all mass flowrate and kinemtaic 
pressure values.
4. be able to deal with hydrostatic pressure and pipe arrangement.
5. validate simuple flow cases for CIET or other flow setups at 
steady state. 


Most of these features are discussed in the pipesAndValues file, rather than

## ensure that we can handle zero and reverse flow

The zero and reverse flow solvers for the friction factor and jacobian are
handled in the isothermal pipe implementation.

```csharp

// Construct the IFrictionFactorJacobian object
_jacobianObject = new StabilisedChurchillJacobian();

```

This is the StabilisedChurchillJacobian class. 

```csharp

[Fact]
public void When_IsothermalPipeNegativePressureExpectNoError(){

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

```

So far, the isothermal pipe seems to hold up well when tested.

```zsh
 The reference Mass flowrate is: 3660 kg/s
IsothermalPipe NegativePressure Verification: 
3723.3071369684676, IsothermalPipe NegativePressure Verification: 
3659.999407477403, 
```

To ensure that the simulation can handle zero pressure drop
refer to the next section: where i use zero pressure drop
as an input parameter to an operating point simulation.




## ensure that we are only using steady state simulations

Previously, i didn't use operating point simulations successfully because 
they didn't print results. Now it's ok:

```csharp
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
	testPipe.Parameters.A = 2.0e3;
	testPipe.Parameters.B = 0.5; 

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
```

The results are:

```zsh
 The reference flowrate for steadyStateSimTest is: 3660 kg/s
steadyStateSimTestOperatingPoint: 
3659.999407477403, 
 The reference flowrate for steadyStateSimTest is: -3.372e-14 kg/s
steadyStateSimTestOperatingPoint: 
-3.37236795406772E-14, 
 The reference flowrate for steadyStateSimTest is: -3660 kg/s
steadyStateSimTestOperatingPoint: 
3659.999407477403
```

So it looks like the test works very well. One is that DC simulation
is no longer needed to facilitate steady state tests, and two that
flowrates near or equal to zero are numerically stable. Also, operating
in negative flowrate region is also numerically okay. However, we will 
not get the negative sign as the output.

## ensuring that we can pass values out of the steady State simulation

For this, i'm not really sure how to do this elegantly. But a simple
solution would be to create my own steady state simulation class.

I'll just call it flowCircuitSteadyState. And the first version is
called MockFlowCircuitSteadyState. Which is nothing but a rename of
the operating point class found in OP.cs.

```zsh

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
	testPipe.Parameters.A = 2.0e3;
	testPipe.Parameters.B = 0.5; 

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
```

This gives me confidence that i can make a new simulation class without
a hitch.

It is the same test and doesn't really do anything new. It ran 
successfully.

Then to start experimenting i have PrototypeSteadyStateFlowSimulation.cs.

For which i will add an interface ISteadyStateFlowSimulation. This 
interface will allow the user to interact with some class variables
and store simulation data in them to be extracted.

Once the basics are setup, the  I will then set some public 
attributes in the class.  Eg. lists and dictionaries so that
data can be extracted.

```csharp
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
	testPipe.Parameters.A = 2.0e3;
	testPipe.Parameters.B = 0.5; 

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
	Assert.Equal(Math.Abs(massFlowRate.As(MassFlowUnit.SI)),
			massFlowRateTestResult.As(MassFlowUnit.SI),3);

	//throw new Exception();
}
```

So this is the test used to see if we could extract data from
PrototypeSteadyStateFlowSimulation objects. It appears we can!

The test passed. So basically when the event was activated,
I set the simulationObject property simulationResult to the current.

This data persists even after the event is over. And I can simply
extract it out of the simulation object and use it to perform
an Assert Test. Goodbye to print tests!!

## ensuring that pipes can deal with hydrostatic pressure

I want to test for zero flow for an incline angle.

At pressure 1.45 $m^2/s^2$, the z height is pretty much

$$z = \frac{1.45 m^2 s^{-2}}{9.81 m s^{-2}}$$
$$z = 0.147808\ m$$

And the incline angle to give zero flow is:

$$\theta = \arcsin \frac{z}{L} = \arcsin \frac{0.147808}{10}$$
$$\theta = 0.01478 radians$$
$$\theta = 0.84691 degrees$$

So i will input 0.84691 degrees as the incline angle and hopefully
that will stop any flow coming in at 1.45 m^2/s^2.

Isothermal pipe should be able to deal with inclines


```csharp
[Fact]
public void WhenInclinedToZeroPressureDrop_ExpectNoFlow(){

	// Setup

	// pressure drop set in m^2/s^2
	double pressureDrop = 1.45;

	// for 10m long pipe, angle of incline for zero pressure drop
	// is about 0.84691 degrees
	Angle inclinedAngle = new Angle(0.846910353, AngleUnit.Degree);
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
		System.Console.Write("PrototypeSteadyStateFlowSimulation: \n");
		System.Console.Write("{0}, ".FormatString(current));
		steadyStateSim.simulationResult = current;
	};

	// Act
	steadyStateSim.Run(ckt);
	double massFlowrate = steadyStateSim.simulationResult;

	// Assert

	Assert.Equal(0.0, massFlowrate,2);
}
```

The test passed; there was no massFlowrate at the given incline
angle.

Now i can repeat this test for an angle decline and it should
give us around 3660 kg/s at zero pressureDrop.

```csharp

[Theory]
[InlineData(1.45, 0.846910353, 0.0)]
[InlineData(0.0, -0.846910353, 3660)]
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
		System.Console.Write("PrototypeSteadyStateFlowSimulation: \n");
		System.Console.Write("{0}, ".FormatString(current));
		steadyStateSim.simulationResult = current;
	};

	// Act
	steadyStateSim.Run(ckt);
	double massFlowrate = steadyStateSim.simulationResult;

	// Assert

	Assert.Equal(expectedFlowrateKgPerSecond, 
			massFlowrate,2);
}
```

This test passed.

I also took the liberty of repeating these with 
tests with obtuse angles:

```csharp

[Theory]
[InlineData(1.45, 0.846910353, 0.0)]
[InlineData(0.0, -0.846910353, 3660)]
[InlineData(1.45, 180-0.846910353, 0.0)]
[InlineData(0.0, 180+0.846910353, 3660)]
```
