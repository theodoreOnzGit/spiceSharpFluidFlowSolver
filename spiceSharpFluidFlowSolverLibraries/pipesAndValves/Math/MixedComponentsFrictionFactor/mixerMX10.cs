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


	// These are specifications of area and hydraulic diameter
	// so that we can calculate Re easily
	// or get mass flowrate from Re easily
	public Area XSArea { get; set; } = new Area(
			6.11e-4, AreaUnit.SquareMeter);

	public Length hydraulicDiameter { get; set; } = new Length(
			2.79e-2, LengthUnit.Meter);


}

