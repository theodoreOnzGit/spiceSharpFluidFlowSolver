// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using EngineeringUnits;
using EngineeringUnits.Units;


public class StabilisedChurchillJacobian : ChurchillMathNetDerivative,
	IFrictionFactorDerivatives, IFrictionFactorJacobian
{
	public double dB_dRe(double Re, double roughnessRatio,
			double lengthToDiameter){

		double lengthToDiameterTerm;
		lengthToDiameterTerm = Math.Pow(4.0*lengthToDiameter,
				3.0);


		//
		// firstly i need to use the derivative object

		IDerivative derivativeObj;
		derivativeObj = new MathNetDerivatives();

		// secondly i need a function with a double
		// in and out
		// this is the function that returns the Reynold's number
		// however, the roughness ratio is kept constant

		this.roughnessRatio = roughnessRatio;

		double constantRoughnessFanningReSq(double Re){
			
			double fanningReSq = this.fanning(Re, this.roughnessRatio)*
				Math.Pow(Re,2.0);

			return fanningReSq;
		}

		// now let's calculate the derivative at a specific Re

		double derivativeResult;
		derivativeResult = derivativeObj.calc(
				constantRoughnessFanningReSq, Re);

		// after i'm done, clean up the roughness ratio
		// variable within the class

		this.roughnessRatio = 0.0;


		derivativeResult *= lengthToDiameterTerm;
		derivativeResult /= 32.0;

		return derivativeResult;


	}

	public SpecificEnergy dDeltaP_dRe(double Re, double roughnessRatio,
			double lengthToDiameter,
			Length lengthScale,
			KinematicViscosity nu){

		lengthScale = lengthScale.ToUnit(LengthUnit.SI);
		nu = nu.ToUnit(KinematicViscosityUnit.SI);
		// dDeltaP_dRe will be in specific energy
		// SI unit is: m^2/s^2 
		// this is the same unit as kinematic pressure
		SpecificEnergy derivativeResult;

		// the type will be unknown unit
		var intermediateUnitResult = nu.Pow(2)/lengthScale.Pow(2);
		intermediateUnitResult *= this.dB_dRe(Re,roughnessRatio,
				lengthToDiameter);

		// after which we transform it to a base unit
		derivativeResult = (SpecificEnergy)intermediateUnitResult;

		return derivativeResult;
	}
	
	public MassFlow dmdRe(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter){

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
			Length pipeLength){

		pressureDrop = pressureDrop.ToUnit(SpecificEnergyUnit.SI);
		fluidKinViscosity = fluidKinViscosity.ToUnit(KinematicViscosityUnit.SI);
		pipeLength = pipeLength.ToUnit(LengthUnit.SI);


		double finalValue;

		finalValue = pressureDrop.As(SpecificEnergyUnit.SI);
		finalValue *= Math.Pow(
				pipeLength.As(
					LengthUnit.SI)
				,2.0);

		finalValue /= Math.Pow(
				fluidKinViscosity.As(
					KinematicViscosityUnit.SI)
				,2.0);

		return finalValue;

	}

	// the following section contains code which calculates
	// the actual jacobians

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

		double lengthToDiameter;
		lengthToDiameter = pipeLength.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		SpecificEnergy dDeltaP_dRe = this.dDeltaP_dRe(Re, 
				roughnessRatio,
				lengthToDiameter,
				pipeLength,
				fluidKinViscosity);

		derivativeResult *= dmdRe.As(MassFlowUnit.SI);
		derivativeResult /= dDeltaP_dRe.As(MassFlowUnit.SI);

		return derivativeResult;
	}

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

		double lengthToDiameter;
		lengthToDiameter = pipeLength.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		SpecificEnergy dDeltaP_dRe = this.dDeltaP_dRe(Re, 
				roughnessRatio,
				lengthToDiameter,
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

		double lengthToDiameter;
		lengthToDiameter = pipeLength.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		double Re = this.getRe(Be,roughnessRatio,
				lengthToDiameter);

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

		double lengthToDiameter;
		lengthToDiameter = pipeLength.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		double Re = this.getRe(Be,roughnessRatio,
				lengthToDiameter);

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
	// these overloads autocalculate and 
	// convert units of absolute roughness for you
	
	public double dm_dPA(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			Length absoluteRoughness,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){

		double roughnessRatio;

		roughnessRatio = absoluteRoughness.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		double derivativeResult;
		derivativeResult = this.dm_dPA(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter,
				pressureDrop,
				roughnessRatio,
				pipeLength,
				fluidKinViscosity);
		return derivativeResult;
	}

	public double dm_dPB(Area crossSectionalArea,
			DynamicViscosity fluidViscosity,
			Length hydraulicDiameter,
			SpecificEnergy pressureDrop,
			Length absoluteRoughness,
			Length pipeLength,
			KinematicViscosity fluidKinViscosity){

		double roughnessRatio;

		roughnessRatio = absoluteRoughness.As(LengthUnit.SI)/
			hydraulicDiameter.As(LengthUnit.SI);

		double derivativeResult;
		derivativeResult = this.dm_dPB(crossSectionalArea,
				fluidViscosity,
				hydraulicDiameter,
				pressureDrop,
				roughnessRatio,
				pipeLength,
				fluidKinViscosity);
		return derivativeResult;
	}





}

