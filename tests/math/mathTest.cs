using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

using spiceSharpFluidFlowSolverLibraries;

namespace tests;

public class mathTest : testOutputHelper
{
	public mathTest(ITestOutputHelper outputHelper):base(outputHelper){

		// this constructor is just here to load the test output helper
		// which is just an object which helps me print code
		// when i run
		//' dotnet watch test --logger "console;verbosity=detailed"
	}

	// this tests for derivatives
	// https://stackoverflow.com/questions/64679421/can-i-pass-a-deligate-to-a-xunit-theory
	// andrewlock.net/creating-parameterised-tests-in-xunit-with-inlinedata-classdata-and-memberdata
	

	public double a;

	public double logarithm_e(double x){
		return Math.Log(x);
	}


	public double cubic(double x){
		return x*x*x*this.a;
	}

	[Fact]
	public void Test_centralDifferenceShouldThrowUndefinedError(){
		// Setup
		IDerivative derivativeObj;
		derivativeObj = new CentralDifference();

		double x = 0.0;
		Func<double, double> Fx = this.logarithm_e;
		// Act
		//
		try
		{
			double dydxActual = derivativeObj.calc(Fx,x);
		}
		catch (Exception e)
		{
			this.cout(e.Message);
			// this type is useful for void returns
			Assert.Throws<DivideByZeroException>(() => derivativeObj.calc(Fx,x));
		}

	}


	[Theory]
	[InlineData(5,5)]
	[InlineData(5,375)]
	[InlineData(144,5)]
	public void Test_centralDifferenceShouldApproximateCubic(double x,
			double a){

		// Setup
		IDerivative derivativeObj;
		derivativeObj = new CentralDifference();
		this.a = a;
		// differentiating a*x^3
		// should yield
		// 3a*x^2
		double dydxExpected = 3*a*Math.Pow(x,2);

		Func<double, double> Fx = this.cubic;
		// Act
		double dydxActual = derivativeObj.calc(Fx,x);

		double relativeError;
		double relativeErrorTolerance = 1e-8;

		relativeError = Math.Abs(dydxActual-dydxExpected)/Math.Abs(dydxExpected);

		// Assert
		//

		Assert.True(relativeError < relativeErrorTolerance);






	}

	
	public double linear(double x)
	{
		return x*this.a;
	}
	[Theory]
	[InlineData(5,5)]
	[InlineData(5,1)]
	[InlineData(5,10)]
	[InlineData(5,5)]
	[InlineData(20.2,2)]
	[InlineData(5,20.2)]
	public void Test_centralDifferenceShouldApproximateLinear(double x,
			double a){

		// Setup
		IDerivative derivativeObj;
		derivativeObj = new CentralDifference();
		this.a = a;
		double dydxExpected = a;

		Func<double, double> Fx = this.linear;
		// Act
		double dydxActual = derivativeObj.calc(Fx,x);

		double relativeError;
		double relativeErrorTolerance = 1e-8;

		relativeError = Math.Abs(dydxActual-dydxExpected)/Math.Abs(dydxExpected);

		// Assert
		//

		Assert.True(relativeError < relativeErrorTolerance);





	}
}
