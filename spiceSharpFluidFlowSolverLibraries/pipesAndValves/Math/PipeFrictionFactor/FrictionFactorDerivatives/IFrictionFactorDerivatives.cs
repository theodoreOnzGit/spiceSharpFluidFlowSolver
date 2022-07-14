// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;


public interface IFrictionFactorDerivatives
{
	double calculateFanningPartialDerivative(double Re, 
			double roughnessRatio);
	double calculateMoodyPartialDerivative(double Re, 
			double roughnessRatio);
	double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio);
}

