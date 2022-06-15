// Here is a class for derivatives
// step size is determined dynamically by specifying a tolerance
// I'll specify it though, the user doesn't get to

using System;

public interface IDerivative
{
	double calc(Func<double,double> Fn, double x);
}
