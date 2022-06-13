// Here is a class for derivatives
// I use MathNet.Numerics libraries in this case

using System;


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
	
	private double stepsize;
	private double stepsizeNew;

	public double calc(Func<double,double> Fn, double x){
		
		// before we even start calculating, we want to error check
		// that if the function is somehow undefined at x,
		// we want an error to be thrown

		double valueOfFx = Fn(x);
		try
		{
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

		// the checkNumberValidity function checks if the number
		// is NaN, positive or negative infinity
		// it throws an exception if that's the case

		// first let's define a stepsize, 1e-4
		this.stepsize = 1e-4;
		this.stepsizeNew = this.stepsize/2;

		double dydx = this.centralDifferenceApproximation(Fn, this.stepsize, x);

		// now let's initiate a while loop

		// and of course i'll have a max iterations
		// to snap us out of that loop
		// if something goes wrong

		double dydxNew = this.centralDifferenceApproximation(Fn, this.stepsizeNew, x);

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

			this.stepsize = this.stepsize/2;
			this.stepsizeNew = this.stepsizeNew/2;
			dydx = this.centralDifferenceApproximation(Fn, this.stepsize, x);
			dydxNew = this.centralDifferenceApproximation(Fn, this.stepsizeNew, x);

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

		// the basic idea is that if the supplied stepsize causes
		// a bad number, then reduce the step size
		// do for about 500 times, then throw a timeout error

		int maxIter = 500;
		int loopNumber = 0;

		// now i'm going to define my yPlus and yMinus
		// and perform a validity check
		double yPlus = Fn(x+stepsize/2);
		double yMinus = Fn(x-stepsize/2);

		// now i will perform the validity check and
		// if either of the checks fail, reduce the stepsize
		//
		// now if the loop number is less than max iterations
		// i will try the number validity
		// loop, 

		while (loopNumber < maxIter)
		{
			
			try
			{
				this.checkNumberValidity(yPlus);
				this.checkNumberValidity(yMinus);
				// if both check out, return the right value
				return this.centralDifferenceApproximationBase(Fn,stepsize,x);
			}
			catch (DivideByZeroException e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Reducing Stepsize to Avoid undefined error...");
				// i will then reduce both old and new stepsize by 2
				this.stepsize = this.stepsize/2;
				this.stepsizeNew = this.stepsizeNew/2;
				// after this evaluate yPlus and yMinus again
				yPlus = Fn(x+this.stepsize/2);
				yMinus = Fn(x-this.stepsize/2);
				// increase loop counter by 1
				loopNumber += 1;
				// then pass control out to the while loop
				Console.WriteLine("Iteration " + loopNumber.ToString());

			}
		}

		string errorMsg = "";
		errorMsg += "autostepSize maxiumum iterations reached...\n";
		errorMsg += "You are trying to perform differentiation in an \n";
		errorMsg += "region where the function is either undefined \n";
		errorMsg += "or not well behaved \n";
		throw new TimeoutException(errorMsg);


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

