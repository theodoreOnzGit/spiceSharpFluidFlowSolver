// Here is a class for Fanning Friction Factor using Churchill's Correlation
//

using System;
using MathNet.Numerics;


public interface IFrictionFactorDerivatives
{
	double calculateFanningPartialDerivative(double Re, 
			double roughnessRatio);
	double calculateMoodyPartialDerivative(double Re, 
			double roughnessRatio);
	double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio);
}

public partial class ChurchHillFrictionFactor : IFrictionFactorDerivatives
{


	public virtual double calculateFanningPartialDerivative(double Re, 
			double roughnessRatio){
		//
		// firstly i need to use the derivative object

		IDerivative derivativeObj;
		derivativeObj = new CentralDifference();

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

	public virtual double calculateMoodyPartialDerivative(double Re, 
			double roughnessRatio){
		//
		// firstly i need to use the derivative object

		IDerivative derivativeObj;
		derivativeObj = new CentralDifference();

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

	public virtual double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio){
		return this.calculateMoodyPartialDerivative(Re, roughnessRatio);
	}


}

public class ChurchillAnalyticalDerivative : 
	ChurchHillFrictionFactor,IFrictionFactorDerivatives
{
	public override double calculateFanningPartialDerivative(
			double Re, double roughnessRatio){

		double derivativeResult;
		derivativeResult = this.partialDerivativeFanningReSquared(Re,
				roughnessRatio);

		derivativeResult -= 2.0 * Re * this.fanning(Re,roughnessRatio);

		derivativeResult /= Math.Pow(Re,2.0);

		return derivativeResult;
	}


	public double partialDerivativeFanningReSquared(
			double Re, double roughnessRatio){

		double finalValue;
		finalValue = 1.0/6.0;
		finalValue *= this.dG1_dRe(Re, roughnessRatio);
		finalValue /= this.G1(Re,roughnessRatio);
		finalValue *= Math.Pow(this.G1(Re,roughnessRatio),-1.0/12.0);
		return finalValue;
	}

	public double dB_dRe(double Re){
		return -this.B(Re) * 16.0 / Re;
	}

	public double dG2_dRe(double Re){

		return -5.1859789 * Math.Pow(Re,-1.9);
	}

	public double G2(double Re, double roughnessRatio){

		double g2value;
		g2value = Math.Pow(7.0/Re, 0.9);
		g2value += 0.27*roughnessRatio;

		return g2value;

	}


	public double dA_dRe(double Re, double roughnessRatio){
		double dAdReValue;
		dAdReValue = 16*this.A(Re,roughnessRatio);
		dAdReValue *= this.dG2_dRe(Re);
		dAdReValue /= this.G2(Re,roughnessRatio);
		dAdReValue /= Math.Log(this.G2(Re,roughnessRatio));

		return dAdReValue;
	}

	public double G1(double Re, double roughnessRatio){

		double g1value;
		g1value = Math.Pow(8.0*Re,12.0);
		double g1RHSvalue = Math.Pow(Re,16.0*3.0/2.0);
		g1RHSvalue *= Math.Pow(
				this.A(Re,roughnessRatio)
				+this.B(Re),
				-3.0/2.0);

		g1value += g1RHSvalue;

		return g1value;
	}


	public double dG1_dRe(double Re, double roughnessRatio){

		double finalValue;
		finalValue = -Math.Pow(Re,16.0) *
			(this.dA_dRe(Re, roughnessRatio) + this.dB_dRe(Re));
		double AplusB = this.A(Re,roughnessRatio) + this.B(Re);
		
		finalValue += 16.0 * Math.Pow(Re,15.0) *
			(AplusB);

		finalValue /= Math.Pow(AplusB,2.0);

		finalValue *= 3.0/2.0 * Math.Pow(Re,16.0/2.0);

		finalValue /= Math.Pow(AplusB,1.0/2.0);

		// this big number is 8^12 * 12
		finalValue += 824633720832  * Math.Pow(Re,11.0);

		AplusB = 0.0;


		return finalValue;

	}

}

public class ChurchillMathNetDerivative : ChurchHillFrictionFactor,
	IFrictionFactorDerivatives
{


	public override double calculateFanningPartialDerivative(double Re, 
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

	public override double calculateMoodyPartialDerivative(double Re, 
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

	public override double calculateDarcyPartialDerivative(double Re, 
			double roughnessRatio){
		return this.calculateMoodyPartialDerivative(Re, roughnessRatio);
	}


}
