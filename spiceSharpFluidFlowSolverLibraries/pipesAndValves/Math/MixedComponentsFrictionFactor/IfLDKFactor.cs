// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;

public interface IfLDKFactor
{

	double fLDK(double ReynoldsNumber);
	double fLDK_ReSq(double ReynoldsNumber);
}
public interface IfLDKFactorGetRe
{

	double getRe(double Be);
}
