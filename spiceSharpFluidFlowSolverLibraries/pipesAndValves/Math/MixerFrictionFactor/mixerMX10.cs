// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using EngineeringUnits;
using EngineeringUnits.Units;


public partial class mixerMX10 : IfLDKFactor
								 ,IfLDKFactorGetRe
{

	public double generic_fLDK(double ReynoldsNumber, double roughnessRatio,
			double lengthToDiameter,
			double K){
		
		double genericPipeFLDK;

		genericPipeFLDK = this.generic_fLDK_ReSq(ReynoldsNumber,roughnessRatio,
				lengthToDiameter,K)/Math.Pow(ReynoldsNumber,2.0);

		return genericPipeFLDK;
	}

	public double fLDK(double ReynoldsNumber){
		// note: as shown previously from pipe tests
		// this FLDK has an issue with flows near zero,
		// we need to give it pipe the ReSq treatment

		return this.fLDK_ReSq(ReynoldsNumber)/Math.Pow(ReynoldsNumber,2.0);
	}

	public double fLDK_ReSq(double ReynoldsNumber){

		double specificMX10FLDK_ReSq;

		// the original correlation is:
		// 21 + 4000/Re
		// from here we can see K = 21.0
		// and if we were to use the 16/Re formula,
		// 4L/D = 4000/16 = 250
		// Otherwise the 64/Re formula
		// L/D = 4000/64 = 62.5
		// if so desired, we can use the generic_fLDK_ReSq formula

		specificMX10FLDK_ReSq = 21.0*ReynoldsNumber*ReynoldsNumber;
		specificMX10FLDK_ReSq += 4000.0*ReynoldsNumber;

		return specificMX10FLDK_ReSq;

	}

	public double generic_fLDK_ReSq(double ReynoldsNumber, double roughnessRatio,
			double lengthToDiameter,
			double K){


		// now the graph in the negative x direction should be a 
		// reflection of the graph in positive x direction along
		// the y axis. So this is using absolute value of Re


		ReynoldsNumber = Math.Abs(ReynoldsNumber);

		// the fanning friction factor function (this.fanning)
		// can return Re values for various values in turbulent as
		// well as laminar region
		//
		// However, if Re is close to zero, 
		// the function is not well behaved
		//
		// so we need to make some adjustments
		//
		// So if Re>1800 we return the traditional fanning formula

		double transitionPoint = 1800.0;
		double fLDK_ReSq;

		if (ReynoldsNumber > transitionPoint)
		{
			double fanningReynoldsNumberSq = this.fanning(ReynoldsNumber, 
					this.roughnessRatio)*
				Math.Pow(ReynoldsNumber,2.0);

			fLDK_ReSq = fanningReynoldsNumberSq*4.0*lengthToDiameter+
				K*Math.Pow(ReynoldsNumber,2.0);

			return fLDK_ReSq;
		}

		// otherwise we return 16/Re*Re^2 or 16*Re
		//
		IInterpolation _linear;

		IList<double> xValues = new List<double>();
		IList<double> yValues = new List<double>();
		xValues.Add(0.0);
		xValues.Add(transitionPoint);

		yValues.Add(0.0);
		yValues.Add(this.fanning(transitionPoint,this.roughnessRatio)*
				Math.Pow(transitionPoint,2.0));

		_linear = Interpolate.Linear(xValues,yValues);

		fLDK_ReSq = _linear.Interpolate(ReynoldsNumber)*4.0*lengthToDiameter;
		fLDK_ReSq += K*Math.Pow(ReynoldsNumber,2.0);


		return fLDK_ReSq;

		// my only concern here is a potential problem if the root is exactly at
		// Re = 1800
	}

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



	public double getRe(double Be){

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
			double fanningTerm;

			fanningTerm = this.fLDK_ReSq(Re);

			//  BejanTerm
			double bejanTerm;
			bejanTerm = 2.0 * this.bejanNumber;

			// to set this to zero, we need:
			return fanningTerm - bejanTerm;

		}

		double ReynoldsNumber;
		ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 1, 1e8);

		// once I'm done, i want to clean up all terms
		this.bejanNumber = 0.0;


		// then let's return Re

		if (isNegative)
		{
			return -ReynoldsNumber;
		}

		return ReynoldsNumber;
	}



	public double bejanNumber;
	public double roughnessRatio;
	public double lengthToDiameter;
	public double K;


	public double generic_getRe(double Be, double roughnessRatio,
			double lengthToDiameter,
			double K){

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
	

		this.bejanNumber = Be;
		this.roughnessRatio = roughnessRatio;
		this.lengthToDiameter = lengthToDiameter;
		this.K = K;

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
			double fanningTerm;

			fanningTerm = this.generic_fLDK_ReSq(Re,this.roughnessRatio,
					this.lengthToDiameter,this.K);

			//  BejanTerm
			double bejanTerm;
			bejanTerm = 2.0 * this.bejanNumber;

			// to set this to zero, we need:
			return fanningTerm - bejanTerm;

		}

		double ReynoldsNumber;
		ReynoldsNumber = FindRoots.OfFunction(pressureDropRoot, 1, 1e8);

		// once I'm done, i want to clean up all terms
		this.bejanNumber = 0.0;
		this.roughnessRatio = 0.0;
		this.lengthToDiameter = 0.0;
		this.K = 0.0;


		// then let's return Re

		if (isNegative)
		{
			return -ReynoldsNumber;
		}

		return ReynoldsNumber;
	}










}

