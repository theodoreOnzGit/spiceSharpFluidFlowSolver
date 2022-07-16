using System;
using MathNet.Numerics;
using EngineeringUnits;
using EngineeringUnits.Units;

public interface IfLDKFactorJacobian : IfLDKFactorGetRe
{
	double dB_dRe(double Re);

	double getBejanNumber(SpecificEnergy pressureDrop,
			KinematicViscosity fluidKinViscosity,
			Length pipeLength);

	MassFlow dmdRe(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter);

	SpecificEnergy dDeltaP_dRe(double Re,
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

}

