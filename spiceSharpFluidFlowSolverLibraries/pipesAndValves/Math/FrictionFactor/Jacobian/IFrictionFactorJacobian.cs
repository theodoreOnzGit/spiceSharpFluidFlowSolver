// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using EngineeringUnits;
using EngineeringUnits.Units;

public interface IFrictionFactorJacobian
{
	double dB_dRe(double Re, double roughnessRatio,
			double lengthToDiameter);

	SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
			double lengthToDiameter,
			Length lengthScale,
			KinematicViscosity nu);

	
}

