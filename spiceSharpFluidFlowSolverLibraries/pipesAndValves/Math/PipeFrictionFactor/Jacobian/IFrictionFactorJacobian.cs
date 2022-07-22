// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using EngineeringUnits;
using EngineeringUnits.Units;

public interface IFrictionFactorJacobian : IFrictionFactorGetRe,
	   IFrictionFactor
{
	double dB_dRe(double Re, double roughnessRatio,
			double lengthToDiameter);

	double getBejanNumber(SpecificEnergy pressureDrop,
			KinematicViscosity fluidKinViscosity,
			Length pipeLength);

	MassFlow dmdRe(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter);

	SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
			double lengthToDiameter,
			Length lengthScale,
			KinematicViscosity nu);

	double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);

	double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);

	double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			double Re,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);

	double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			double Re,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);

	double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			Length absoluteRoughness,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);

	double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			Length absoluteRoughness,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity);
}

