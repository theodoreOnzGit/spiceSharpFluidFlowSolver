// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;

public class ChurchHillFrictionFactor : IFrictionFactor
{
	// this particular implementation uses the churchill correlation
	public double fanning(double ReynoldsNumber, double roughnessRatio){
		return 0.0;
	}

	public double moody(double ReynoldsNumber, double roughnessRatio){
		return 0.0;
	}
	
	public double darcy(double ReynoldsNumber, double roughnessRatio){
		return 0.0;
	}

	private double A(double Re, double roughnessRatio){
		return 0.0;
	}

	private double B(double Re){
		double numerator = Math.Pow(37530,16);
		double denominator = Math.Pow(Re,16);
		return numerator/denominator;
	}


}

public interface IFrictionFactor
{
	double fanning(double ReynoldsNumber, double roughnessRatio);
	double moody(double ReynoldsNumber, double roughnessRatio);
	double darcy(double ReynoldsNumber, double roughnessRatio);
}
