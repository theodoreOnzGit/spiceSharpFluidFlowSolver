// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;



public partial class TsalFrictionFactor : IFrictionFactor
{

	public double fanning(double ReynoldsNumber, double roughnessRatio){
		return 0.25*this.darcy(ReynoldsNumber,roughnessRatio);
	}

	public double moody(double ReynoldsNumber, double roughnessRatio){
		return this.darcy(ReynoldsNumber,roughnessRatio);
	}
	
	public double darcy(double ReynoldsNumber, double roughnessRatio){
		// https://iwaponline.com/ws/article/20/4/1321/73330/Approximations-of-the-Darcy-Weisbach-friction
		//
		double C;

		C = Math.Pow(68.0/ReynoldsNumber + roughnessRatio, 0.25);
		C *= 0.11;

		if (C >= 0.018)
		{
			return C;			
		}

		if (C < 0.018)
		{
			return 0.0028 + 0.85*C;
		}

		string exceptionMsg = "";
		exceptionMsg += "\n darcy friction factor is negative";
		exceptionMsg += "\n check if ReynoldsNumber or roughnessRatio";
		exceptionMsg += "\n supplied is negative or NaN";

		throw new Exception();

	}

}

