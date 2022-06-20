// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;


public interface IFrictionFactorGetRe
{
	double getRe(double Be, double roughnessRatio, 
			double lengthToDiameter);
}

public partial class ChurchHillFrictionFactor : IFrictionFactor
												,IFrictionFactorGetRe
{
	// this particular implementation uses the churchill correlation
	public double fanning(double ReynoldsNumber, double roughnessRatio){

		double fanningFrictionFactor;
		fanningFrictionFactor = 2 * Math.Pow(this.churchillInnerTerm(ReynoldsNumber,roughnessRatio), 1.0/12);
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
		laminarTerm = Math.Pow(8.0/Re, 12);

		double turbulentTerm;
		double Aterm = this.A(Re,roughnessRatio);
		double Bterm = this.B(Re);

		turbulentTerm = Math.Pow( 1.0/(Aterm + Bterm), 3.0/2);

		return laminarTerm + turbulentTerm;


	}

	public double A(double Re, double roughnessRatio){
		// first i need the logarithm of a number

		double reynoldsTerm =  Math.Pow( (7.0/Re), 0.9);
		double roughnessTerm = 0.27*roughnessRatio;

		double logFraction = 1.0/(reynoldsTerm+roughnessTerm);
		double innerBracketTerm = 2.457*Math.Log(logFraction);
		double A = Math.Pow(innerBracketTerm,16);
		
		return A;
	}

	public double B(double Re){
		double numerator = Math.Pow(37530,16);
		double denominator = Math.Pow(Re,16);
		return numerator/denominator;
	}
	/*
	 ************************************************************* 
	 ************************************************************* 
	 ************************************************************* 
	 this part will help implement code to find Re given a specific
	 Bejan number and roughnessRatio
	 ************************************************************* 
	 ************************************************************* 
	 ************************************************************* 
	 ************************************************************* 
	 */


	 
	public double getRe(double Be, 
			double roughnessRatio,
			double lengthToDiameter){

		this.roughnessRatio = roughnessRatio;
		this.lengthToDiameter = lengthToDiameter;
		this.bejanNumber = Be;

		// I'll define a pressureDrop function with which to find
		// the Reynold's Number
		double pressureDropRoot(double Re){

			// fanning term
			//
			double fanningTerm;
			fanningTerm = this.fanning(Re, this.roughnessRatio);
			fanningTerm *= Math.Pow(Re,2.0);


			//  BejanTerm
			//
			double bejanTerm;
			bejanTerm = 32.0 * this.bejanNumber;
			bejanTerm *= Math.Pow(4.0*this.lengthToDiameter,-3);

			// to set this to zero, we need:
			//
			return fanningTerm - bejanTerm;

		}

		double ReynoldsNumber;
		ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 1, 1e8);

		// once I'm done, i want to clean up all terms
		this.roughnessRatio = 0.0;
		this.lengthToDiameter = 0.0;
		this.bejanNumber = 0.0;


		// then let's return Re

		return ReynoldsNumber;
	}



	public double roughnessRatio;
	public double lengthToDiameter;
	public double bejanNumber;












}

