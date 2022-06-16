# Derivatives Readme

The derivative class is here to perform numerical differentiation.

# IDerivative

Since there are many ways of performing numerical differentiation,
I gave an IDerivative interface

```csharp

using System;

public interface IDerivative
{
	double calc(Func<double,double> Fn, double x);
}
```
Derivative classes can use this IDerivative interface.

## Design

# CentralDifference (in house code)

```csharp

private double centralDifferenceApproximationBase(Func<double,double> Fn,
		double stepsize, double x){
	double yPlus = Fn(x+stepsize/2);
	double yMinus = Fn(x-stepsize/2);
	double dydxApprox = (yPlus - yMinus)/stepsize;
	return dydxApprox;
}
```

I didn't want the user to think of stepsize, so i have an inhouse
stepsize.

```csharp

// first let's define a stepsize, 1e-4
this.stepsize = 1e-4;
this.stepsizeNew = this.stepsize/2;
```

I defined an initial stepsize.

I designed the loop to differentiate, and reduce stepsize until
some convergence is reached.

```csharp

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
```

I have a while loop to do this for me:


```csharp

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
```
I have a lot of checks to see if the derivative goes to
infinity

```csharp

try
{
	this.checkNumberValidity(dydx);
	return dydx;
}
```

You can read it in your own time.
