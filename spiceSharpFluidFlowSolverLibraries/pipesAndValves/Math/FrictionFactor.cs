// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;

public class ChurchHillFrictionFactor : IFrictionFactor
{
	// this particular implementation uses the churchill correlation
	public double fanning(double ReynoldsNumber, double roughnessRatio){

		double fanningFrictionFactor;
		fanningFrictionFactor = 2 * Math.Pow(this.churchillInnerTerm(ReynoldsNumber,roughnessRatio), 1/12);
		return fanningFrictionFactor;
	}

	public double moody(double ReynoldsNumber, double roughnessRatio){

		// apparently the moody friciton factor is same as the darcy friction factor

		return this.darcy(ReynoldsNumber,roughnessRatio);
	}
	
	public double darcy(double ReynoldsNumber, double roughnessRatio){

		// darcy friction factor is 4x fanning friction factor
		// https://neutrium.net/fluid-flow/pressure-loss-in-pipe/
		double darcyFrictionFactor;
		darcyFrictionFactor = 4 * this.fanning(ReynoldsNumber,roughnessRatio);
		return darcyFrictionFactor;
	}



	private double churchillInnerTerm(double Re, double roughnessRatio){

		double laminarTerm;
		laminarTerm = Math.Pow(8/Re, 12);

		double turbulentTerm;
		double Aterm = this.A(Re,roughnessRatio);
		double Bterm = this.B(Re);

		turbulentTerm = Math.Pow( 1/(Aterm + Bterm), 3/2);

		return laminarTerm + turbulentTerm;


	}

	private double A(double Re, double roughnessRatio){
		// first i need the logarithm of a number

		double reynoldsTerm =  Math.Pow(7/Re , 0.9);
		double roughnessTerm = 0.27*roughnessRatio;

		double logFraction = Math.Pow(reynoldsTerm+roughnessTerm,-1);
		double A = 2.457*Math.Log(logFraction);
		
		return A;
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
