// Here is a class for derivatives
// I use MathNet.Numerics libraries in this case

using System;
using MathNet.Numerics;


public class MathNetDerivatives : IDerivative
{
	// this particular implementation uses central difference approximation
	// normally this requires an explicit step size
	// but I intend to determine the step size in an adaptive manner
	// so i'll probably start with a small stepsize, eg. initially 0.5
	// but i find that trying to find derivatives near zero
	// eg log(x) at 0.001
	// is quite troublesome
	// i'd rather reduce the stepsize to 1e-4 from the get go
	// and then decrease it 2x, until convergence is reached
	//
	
	// your job is to pass in the function
	// that calculates a double and returns a double
	

	public double calc(Func<double,double> Fn, double x){
		
		// before we even start calculating, we want to error check
		// that if the function is somehow undefined at x,
		// we want an error to be thrown

		try
		{
			double valueOfFx = Fn(x);
			this.checkNumberValidity(valueOfFx);
		}
		catch (Exception e)
		{
			string exceptionMsg = e.Message;
			exceptionMsg += "\n";
			exceptionMsg += "Function and derivative undefined at:\n";
			exceptionMsg += "x = " + x.ToString() + "\n";
			throw new DivideByZeroException(exceptionMsg);
		}

		double dydx;
		dydx = Differentiate.FirstDerivative(Fn,x);


		// this is the final number check
		try
		{
			this.checkNumberValidity(dydx);
			return dydx;
		}
		catch (Exception e)
		{
			string errorMsg = "";
			errorMsg += e.Message;
			errorMsg += "\n";
			errorMsg += "You are trying to perform differentiation in an \n";
			errorMsg += "region where the function is either undefined \n";
			errorMsg += "or not well behaved \n";
			throw new DerivativeBadlyBehavedException(errorMsg);
		}
	}


	private double centralDifferenceApproximationBase(Func<double,double> Fn,
			double stepsize, double x){
		double yPlus = Fn(x+stepsize/2);
		double yMinus = Fn(x-stepsize/2);
		double dydxApprox = (yPlus - yMinus)/stepsize;
		return dydxApprox;
	}

	// this function here tests for infinity, NaN values
	// it will throw errors if the numbers are positive infinity
	// or negative infinity
	
	private void checkNumberValidity(double number){
		string errorMsg = "";
		if (Double.IsPositiveInfinity(number) | Double.IsNegativeInfinity(number))
		{
			errorMsg += "positive or negative infinity detected,\n";
			errorMsg += "the function you supplied is undefined\n";
			errorMsg += "\n \n";
			throw new DivideByZeroException(errorMsg);
		}
		else if (Double.IsNaN(number))
		{
			errorMsg += "NaN detected";
			errorMsg += "the function you supplied is undefined\n";
			errorMsg += "\n \n";
			throw new DivideByZeroException(errorMsg);
		}
		else
		{
			return;
		}


	}

}

