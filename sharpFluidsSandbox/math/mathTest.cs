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

		// now i'll also create dependencies in the constructor
		// 
		IDerivative derivativeObj = new CentralDifference();
		this._derivativeObj = derivativeObj;
	}

	// this tests for derivatives
	// https://stackoverflow.com/questions/64679421/can-i-pass-a-deligate-to-a-xunit-theory
	// andrewlock.net/creating-parameterised-tests-in-xunit-with-inlinedata-classdata-and-memberdata
	

	public double a;

	private IDerivative _derivativeObj;

	public double logarithm_e(double x){
		return Math.Log(x);
	}


	public double cubic(double x){
		return x*x*x*this.a;
	}

	[Theory(Skip = "deprecated")]
	[InlineData(0.5)]
	[InlineData(0.01)]
	[InlineData(0.1)]
	[InlineData(1e-4/2)]
	public void Test_centralDifferenceShouldReduceStepSizeIfOnePointIsUndefined(
			double x){
		// this test is here to try evaluating Log(x) at x=0.25
		// everything seems okay
		// till you  realise that the central difference
		// approximation has an initial stepsize of 0.5
		// which means an error is bound to occur.
		// i want to make the system smart enough to reduce the stepsize
		// if something like that occurs
		// until a proper answer is reached
		//
		//
		// it appears that for one iteration of reducing stepsize
		// things are okay
		// but after that, the loop doesn't quite work
		// the simple fix is to reduce the initial stepsize to 1e-4
		// if it doesn't get fixed by the time that the stepsize
		// is reduced by half, don't even bother
		// just throw the error
		//
		// so i supplied an initial stepsize at 1e-4
		// and the differentiation was at a point 1e-4/2
		// the thing succeeded. I think that's about sufficient for
		// the purposes of my project

		// Setup

		Func<double, double> Fx = this.logarithm_e;

		double dydxExpected = 1.0/x;
		// Act
		//
		double dydxActual = this._derivativeObj.calc(Fx,x);


		double relativeError;
		double relativeErrorTolerance = 1e-8;

		relativeError = Math.Abs(dydxActual-dydxExpected)/Math.Abs(dydxExpected);

		// Assert
		//

		this.cout("\n");
		this.cout("dydx expected: " + dydxExpected.ToString() +"\n");
		this.cout("dydx actual: " + dydxActual.ToString() +"\n");

		Assert.True(relativeError < relativeErrorTolerance);


	}
	
	[Theory(Skip = "Deprecated")]
	[InlineData(1e-4/4)]
	public void Test_centralDifferenceShouldThrowBadlyBehavedError(
			double x){
		// Setup

		Func<double, double> Fx = this.logarithm_e;
		// Act
		//
		try
		{
			double dydxActual = this._derivativeObj.calc(Fx,x);
		}
		catch (Exception e)
		{
			this.cout(e.Message);
			// this type is useful for void returns
			Assert.Throws<DerivativeBadlyBehavedException>(() => this._derivativeObj.calc(Fx,x));
		}

	}

	[Fact(Skip = "Deprecated")]
	public void Test_centralDifferenceShouldThrowUndefinedError(){
		// Setup

		double x = 0.0;
		Func<double, double> Fx = this.logarithm_e;
		// Act
		//
		try
		{
			double dydxActual = this._derivativeObj.calc(Fx,x);
		}
		catch (Exception e)
		{
			this.cout(e.Message);
			// this type is useful for void returns
			Assert.Throws<DivideByZeroException>(() => this._derivativeObj.calc(Fx,x));
		}

	}


	[Theory(Skip = "Deprecated")]
	[InlineData(5,5)]
	[InlineData(5,375)]
	[InlineData(144,5)]
	public void Test_centralDifferenceShouldApproximateCubic(double x,
			double a){

		// Setup
		this.a = a;
		// differentiating a*x^3
		// should yield
		// 3a*x^2
		double dydxExpected = 3*a*Math.Pow(x,2);

		Func<double, double> Fx = this.cubic;
		// Act
		double dydxActual = this._derivativeObj.calc(Fx,x);

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
	[Theory(Skip = "Deprecated")]
	[InlineData(5,5)]
	[InlineData(5,1)]
	[InlineData(5,10)]
	[InlineData(15,5)]
	[InlineData(20.2,2)]
	[InlineData(5,20.2)]
	public void Test_centralDifferenceShouldApproximateLinear(double x,
			double a){

		// Setup
		this.a = a;
		double dydxExpected = a;

		Func<double, double> Fx = this.linear;
		// Act
		double dydxActual = this._derivativeObj.calc(Fx,x);

		double relativeError;
		double relativeErrorTolerance = 1e-8;

		relativeError = Math.Abs(dydxActual-dydxExpected)/Math.Abs(dydxExpected);

		// Assert
		//

		Assert.True(relativeError < relativeErrorTolerance);





	}
}