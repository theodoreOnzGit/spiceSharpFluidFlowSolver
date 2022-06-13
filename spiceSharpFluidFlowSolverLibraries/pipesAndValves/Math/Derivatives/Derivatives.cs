// Here is a class for derivatives
// step size is determined dynamically by specifying a tolerance
// I'll specify it though, the user doesn't get to

using System;

public interface IDerivative
{
	double calc(Func<double,double> Fn, double x);
}

public class CentralDifference : IDerivative
{
	// this particular implementation uses central difference approximation
	// normally this requires an explicit step size
	// but I intend to determine the step size in an adaptive manner
	// so i'll probably start with a small stepsize, eg. 0.5
	// and then decrease it 2x, until convergence is reached
	//
	
	// your job is to pass in the function
	// that calculates a double and returns a double

	public double calc(Func<double,double> Fn, double x){
		
		// before we even start calculating, we want to error check
		// that if the function is somehow undefined at x,
		// we want an error to be thrown

		double valueOfFx = Fn(x);
		this.checkNumberValidity(valueOfFx);

		// the checkNumberValidity function checks if the number
		// is NaN, positive or negative infinity
		// it throws an exception if that's the case

		// first let's define a stepsize, 0.5 for example
		double stepsize = 0.5;

		double dydx = this.centralDifferenceApproximation(Fn, stepsize, x);

		// now let's initiate a while loop

		// and of course i'll have a max iterations
		// to snap us out of that loop
		// if something goes wrong

		double stepsizeNew = stepsize/10;
		double dydxNew = this.centralDifferenceApproximation(Fn, stepsizeNew, x);

		int maxIter = 1000;
		int loopCount = 0;

		// let's calculate the error using a nested function
		double relativeError(double newValue, double oldValue){
			double absoluteError = Math.Abs(newValue - oldValue);
			return absoluteError/Math.Abs(newValue);
		}

		// i'm going to define a tolerance here
		double error = relativeError(dydxNew,dydx);
		double tolerance = 1.0e-10;

		// while the error is more than tolerance,
		// shrink the stepsize and recalculate everything
		while (error > tolerance)
		{

			stepsize = stepsizeNew;
			stepsizeNew = stepsize/2;
			dydx = this.centralDifferenceApproximation(Fn, stepsize, x);
			dydxNew = this.centralDifferenceApproximation(Fn, stepsizeNew, x);

			// now let's calculate the new error

			error = relativeError(dydxNew,dydx);

			// if the error is less than the tolerance the while loop
			// will exit
			// otherwise keep going
			//
			// We however don't want to be suck in an infinite loop
			// so we increase the loopCount by 1

			loopCount += 1;

			// and if the loopcount is more than 1

			if (loopCount > maxIter)
			{
				string errorMsg;
				errorMsg = "Maximum Iterations reached for derivative";
				throw new TimeoutException(errorMsg);
					
			}
			

		}
		return dydx;
	}

	// under the hood, 
	// i am actually wanting to 
	// make a central difference method
	// which explicitly needs a stepsize

	private double centralDifferenceApproximation(Func<double,double> Fn,
			double stepsize, double x){
		// this function does a few things,
		// besides doing the central difference approximation
		// it will also check to see if the numbers are valid
		// ie are numbers undefined etc.
		// and if so, stepsize should decrease
		return this.centralDifferenceApproximationBase(Fn,stepsize,x);
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
