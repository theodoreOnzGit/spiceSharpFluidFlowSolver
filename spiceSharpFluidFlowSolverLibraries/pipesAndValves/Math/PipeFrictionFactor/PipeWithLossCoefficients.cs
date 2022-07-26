// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;



public partial class PipeWithLossCoefficients : ChurchillFrictionFactor,
	IFrictionFactor,
	IFrictionFactorGetRe,
	IfLDKFactorPipe,
	IfLDKFactorGetRePipe
{
	public double generic_fLDK(double ReynoldsNumber, double roughnessRatio,
			double lengthToDiameter,
			double K){
		
		double genericPipeFLDK;

		genericPipeFLDK = this.generic_fLDK_ReSq(ReynoldsNumber,roughnessRatio,
				lengthToDiameter,K)/Math.Pow(ReynoldsNumber,2.0);

		return genericPipeFLDK;
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



	public new double getRe(double Be, 
			double roughnessRatio,
			double lengthToDiameter){

		// this getRe assumes K=0
		Console.WriteLine("Assuming pipe loss coefficeints set to zero");
		return this.generic_getRe(Be, roughnessRatio, lengthToDiameter, 0.0);
	}


	public new double bejanNumber;
	public new double roughnessRatio;
	public new double lengthToDiameter;
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

