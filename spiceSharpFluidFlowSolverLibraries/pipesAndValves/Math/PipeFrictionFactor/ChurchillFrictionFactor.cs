// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;



public partial class ChurchillFrictionFactor : IFrictionFactor
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

		// now i want to make sure this function can handle negative 
		// pressure drop
		//
		// ie pressure drops in reverse direction, and this should
		// yield us reverse flow and negative Reynold's numbers
		// so what i'll do is this: if Be < 0,
		// then i'll make it positive
		//

		bool isNegative;
		if (Be < 0)
		{
			Be *= -1;
			isNegative = true;
		}
		else 
		{
			isNegative = false;
		}
	

		this.roughnessRatio = roughnessRatio;
		this.lengthToDiameter = lengthToDiameter;
		this.bejanNumber = Be;

		// I'll define a pressureDrop function with which to find
		// the Reynold's Number
		double pressureDropRoot(double Re){

			// fanning term
			//
			//
			// Now here is a potential issue for stability,
			// if Re = 0, the fanning friction factor is not well behaved,
			// Hence it's better to use the laminar term at low Reynold's number
			//
			// we note that in the laminar regime, 
			// f = 16/Re
			// so f*Re^2 = 16*Re
			double transitionPoint = 1800.0;
			double fanningTerm;

			if (Re > transitionPoint)
			{
				fanningTerm = this.fanning(Re, this.roughnessRatio);
				fanningTerm *= Math.Pow(Re,2.0);
			}
			else
			{
				// otherwise we return 16/Re*Re^2 or 16*Re
				// or rather an interpolated version to preserve the
				// continuity of the points.
				IInterpolation _linear;

				IList<double> xValues = new List<double>();
				IList<double> yValues = new List<double>();
				xValues.Add(0.0);
				xValues.Add(transitionPoint);

				yValues.Add(0.0);
				yValues.Add(this.fanning(transitionPoint,this.roughnessRatio)*
						Math.Pow(transitionPoint,2.0));

				_linear = Interpolate.Linear(xValues,yValues);
				fanningTerm = _linear.Interpolate(Re);
			}






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
		ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 0.001, 1e8);

		// once I'm done, i want to clean up all terms
		this.roughnessRatio = 0.0;
		this.lengthToDiameter = 0.0;
		this.bejanNumber = 0.0;


		// then let's return Re

		if (isNegative)
		{
			return -ReynoldsNumber;
		}

		return ReynoldsNumber;
	}



	public double roughnessRatio;
	public double lengthToDiameter;
	public double bejanNumber;












}

