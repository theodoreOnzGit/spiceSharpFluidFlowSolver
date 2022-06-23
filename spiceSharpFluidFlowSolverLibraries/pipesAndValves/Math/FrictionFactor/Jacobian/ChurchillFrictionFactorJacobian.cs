// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;
using EngineeringUnits;
using EngineeringUnits.Units;


public class ChurchillFrictionFactorJacobian : ChurchillMathNetDerivative,
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

}

