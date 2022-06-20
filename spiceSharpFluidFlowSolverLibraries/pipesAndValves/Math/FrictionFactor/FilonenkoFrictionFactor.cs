// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;



public class FilonenkoFrictionFactor : IFrictionFactor,
	IFrictionFactorDerivatives
{

	public double fanning(double ReynoldsNumber, double roughnessRatio){
		return 0.25*this.darcy(ReynoldsNumber,roughnessRatio);
	}

	public double moody(double ReynoldsNumber, double roughnessRatio){
		return this.darcy(ReynoldsNumber,roughnessRatio);
	}
	
	public double darcy(double ReynoldsNumber, double roughnessRatio){
		// https://iwaponline.com/ws/article/20/4/1321/73330/Approximations-of-the-Darcy-Weisbach-friction
		//
		double sqrt1OverFd;
		sqrt1OverFd = -2.0 * Math.Log10(this.innerLogTerm(
					ReynoldsNumber, roughnessRatio));

		double result;
		result = Math.Pow(sqrt1OverFd,-2.0);

		if(result > 0.0)
		{
			return result;
		}


		string exceptionMsg = "";
		exceptionMsg += "\n darcy friction factor is negative";
		exceptionMsg += "\n check if ReynoldsNumber or roughnessRatio";
		exceptionMsg += "\n supplied is negative or NaN";

		throw new Exception();

	}

	public double innerLogTerm(double ReynoldsNumber, double roughnessRatio){

		double result;
		result = 6.6069/Math.Pow(ReynoldsNumber,0.91);
		result += roughnessRatio/3.71;
		return result;
	}

	// here is the part for derivatives



	public double calculateMoodyPartialDerivative(double Re, 
			double roughnessRatio){
		//
		// firstly i need to use the derivative object

		IDerivative derivativeObj;
		derivativeObj = new MathNetDerivatives();

		// secondly i need a function with a double
		// in and out
		// this is the function that returns the Reynold's number
		// however, the roughness ratio is kept constant

		this.roughnessRatio = roughnessRatio;

		double constantRoughnessMoody(double Re){
			
			double moody = this.moody(Re, this.roughnessRatio);

			return moody;
		}

		// now let's calculate the derivative at a specific Re

		double derivativeResult;
		derivativeResult = derivativeObj.calc(
				constantRoughnessMoody, Re);

		// after i'm done, clean up the roughness ratio
		// variable within the class

		this.roughnessRatio = 0.0;


		return derivativeResult;
	}

	public double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio){
		return this.calculateMoodyPartialDerivative(Re, roughnessRatio);
	}

	public double calculateFanningPartialDerivative(double Re, 
			double roughnessRatio){
		//
		// firstly i need to use the derivative object

		IDerivative derivativeObj;
		derivativeObj = new MathNetDerivatives();

		// secondly i need a function with a double
		// in and out
		// this is the function that returns the Reynold's number
		// however, the roughness ratio is kept constant

		this.roughnessRatio = roughnessRatio;

		double constantRoughnessFanning(double Re){
			
			double fanning = this.fanning(Re, this.roughnessRatio);

			return fanning;
		}

		// now let's calculate the derivative at a specific Re

		double derivativeResult;
		derivativeResult = derivativeObj.calc(
				constantRoughnessFanning, Re);

		// after i'm done, clean up the roughness ratio
		// variable within the class

		this.roughnessRatio = 0.0;


		return derivativeResult;

	}

	private double roughnessRatio;
}

public class FilonenkoAnalyticalDerivative : 
	FilonenkoFrictionFactor,
	IFrictionFactorDerivatives
{
	double calculateFanningPartialDerivative(double Re, 
			double roughnessRatio){

		return this.calculateDarcyPartialDerivative(
				Re, roughnessRatio) * 0.25;
	}


	double calculateMoodyPartialDerivative(double Re, 
			double roughnessRatio){

		return this.calculateDarcyPartialDerivative(
				Re, roughnessRatio);
	}


	double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio){
		double result;
		result = -2.0 * Math.Pow(
				this.darcy(Re,roughnessRatio),1.5);

		result *= -2.0/Math.Log(10);
		result *= (6.6069*-0.91 * Math.Pow(Re,-1.91));
		result /= this.innerLogTerm(Re, roughnessRatio);

		return result;

	}


}
