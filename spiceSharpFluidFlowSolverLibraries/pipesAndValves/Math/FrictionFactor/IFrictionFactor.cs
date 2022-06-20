// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;

public interface IFrictionFactor
{
	double fanning(double ReynoldsNumber, double roughnessRatio);
	double moody(double ReynoldsNumber, double roughnessRatio);
	double darcy(double ReynoldsNumber, double roughnessRatio);
}
