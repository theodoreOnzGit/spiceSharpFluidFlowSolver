using System;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using EngineeringUnits;
using EngineeringUnits.Units;

public class flowmeterFM40Jacobian : flowmeterFM40,
	IfLDKFactor, IfLDKFactorJacobian
{
	public double dB_dRe(double Re){
		// first let's get the derivative object
		IDerivative derivativeObj;
		derivativeObj = new MathNetDerivatives();

		double derivativeResult;
		derivativeResult = derivativeObj.calc(
				this.fLDK_ReSq, Re);
		derivativeResult *= 0.5;

		return derivativeResult;

	}
	public SpecificEnergy dDeltaP_dRe(double Re,
			Length lengthScale,
			KinematicViscosity nu){
		// for FM40, and most other components,
		// the lengthscale here is diameter

		lengthScale = lengthScale.ToUnit(LengthUnit.SI);
		nu = nu.ToUnit(KinematicViscosityUnit.SI);
		// dDeltaP_dRe will be in specific energy
		// SI unit is: m^2/s^2 
		// this is the same unit as kinematic pressure
		SpecificEnergy derivativeResult;

		// the type will be unknown unit
		var intermediateUnitResult = nu.Pow(2)/lengthScale.Pow(2);
		intermediateUnitResult *= this.dB_dRe(Re);

		// after which we transform it to a base unit
		derivativeResult = (SpecificEnergy)intermediateUnitResult;

		return derivativeResult;
	}

	
	public MassFlow dmdRe(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter){

		// dmdRe is here to help us convert derivatives, no change here

		crossSectionalArea = crossSectionalArea.ToUnit(AreaUnit.SI);
		fluidViscosity = fluidViscosity.ToUnit(DynamicViscosityUnit.SI);
		hydraulicDiameter = hydraulicDiameter.ToUnit(LengthUnit.SI);

		var intermediateUnitResult = crossSectionalArea
			*fluidViscosity
			/hydraulicDiameter;

		MassFlow derivativeResult;
		derivativeResult = (MassFlow)intermediateUnitResult;

		return derivativeResult;

	}

	public double dDeltaP_dPA(){
		return 1.0;
	}

	public double dDeltaP_dPB(){
		return -1.0;
	}

	public double getBejanNumber(
			SpecificEnergy pressureDrop,
			KinematicViscosity fluidKinViscosity,
			Length hydraulicDiameter){

		// Be = deltaP * D^2 / nu^2
		// D is hydraulic diameter, nu is kinematic viscosity
		// deltaP is pressureDrop

		pressureDrop = pressureDrop.ToUnit(SpecificEnergyUnit.SI);
		fluidKinViscosity = fluidKinViscosity.ToUnit(KinematicViscosityUnit.SI);
		hydraulicDiameter = hydraulicDiameter.ToUnit(LengthUnit.SI);


		double finalValue;

		finalValue = pressureDrop.As(SpecificEnergyUnit.SI);
		finalValue *= Math.Pow(
				hydraulicDiameter.As(
					LengthUnit.SI)
				,2.0);

		finalValue /= Math.Pow(
				fluidKinViscosity.As(
					KinematicViscosityUnit.SI)
				,2.0);

		return finalValue;

	}

	// reduce useless variables, eg. roughness ratio
	// do later

	public double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			double Re,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){
		

		double derivativeResult;
		derivativeResult = this.dDeltaP_dPA();

		MassFlow dmdRe = this.dmdRe(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter);

		SpecificEnergy dDeltaP_dRe = this.dDeltaP_dRe(Re, 
				pipeLength,
				fluidKinViscosity);

		derivativeResult *= dmdRe.As(MassFlowUnit.SI);
		derivativeResult /= dDeltaP_dRe.As(MassFlowUnit.SI);

		return derivativeResult;
	}

	// reduce useless variables, eg. roughness ratio
	// do later
	public double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			double Re,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){

		double derivativeResult;
		derivativeResult = this.dDeltaP_dPB();

		MassFlow dmdRe = this.dmdRe(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter);

		SpecificEnergy dDeltaP_dRe = this.dDeltaP_dRe(Re, 
				pipeLength,
				fluidKinViscosity);

		derivativeResult *= dmdRe.As(MassFlowUnit.SI);
		derivativeResult /= dDeltaP_dRe.As(MassFlowUnit.SI);

		return derivativeResult;
	}

	// the following overloads of dm_dPA
	// help to find the derivative based on pressure drop
	// rather than Reynold's number
	// So it does the root finding process fist
	// and then returns the value internally for you

	// reduce useless variables, eg. roughness ratio
	// do later
	public double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){

		double Be;
		Be = this.getBejanNumber(pressureDrop,
				fluidKinViscosity,
				pipeLength);

		double Re = this.getRe(Be);

		double derivativeResult;
		derivativeResult = this.dm_dPA(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter,
				Re,
				roughnessRatio,
				pipeLength,
				fluidKinViscosity);
		return derivativeResult;
	}

	// reduce useless variables, eg. roughness ratio
	// do later
	public double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			double roughnessRatio,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){

		double Be;
		Be = this.getBejanNumber(pressureDrop,
				fluidKinViscosity,
				pipeLength);

		double Re = this.getRe(Be);

		double derivativeResult;
		derivativeResult = this.dm_dPB(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter,
				Re,
				roughnessRatio,
				pipeLength,
				fluidKinViscosity);
		return derivativeResult;
	}
}
