// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;

public interface IfLDKFactor
{
	double generic_fLDK(double ReynoldsNumber, double roughnessRatio,
			double lengthToDiameter,
			double K);

	double generic_fLDK_ReSq(double ReynoldsNumber, double roughnessRatio,
			double lengthToDiameter,
			double K);

	double fLDK(double ReynoldsNumber);
	double fLDK_ReSq(double ReynoldsNumber);
}
public interface IfLDKFactorGetRe
{
	double generic_getRe(double Be, double roughnessRatio, 
			double lengthToDiameter,
			double K);

	double getRe(double Be);
}
