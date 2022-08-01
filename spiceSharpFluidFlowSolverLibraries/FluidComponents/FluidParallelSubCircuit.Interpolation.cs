using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharp.Components.Subcircuits;
using System.Linq;
using EngineeringUnits;
using EngineeringUnits.Units;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A subcircuit that can contain a collection of entities.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
	///
	/// Note that this MockFluidSubCircuit class is just a copy of SubCircuit
	/// it doesn't change any code other than the class name,
	/// two overloads of constructors
	///
	/// And also the clone method
	/// which returns a new MockFluidSubCircuit rather than
	/// returning a SubCircuit
    public partial class FluidParallelSubCircuit : FluidEntity, 
	IParameterized<Parameters>,
	IComponent,
	IRuleSubject
    {

		IInterpolation _interpolateBeFromRe;

		public void constructInterpolateReFromBe(){
			// first let me get a set of about 500 values
			// ranging from Re = 0 to Re = 1e12

			IList<double> ReValues = new List<double>();
			IList<double> BeValues = new List<double>();
			// at Re = 0, Be = 0
			//
			ReValues.Add(0.0);
			BeValues.Add(0.0);
			for (int i = 0; i < 500; i++)
			{
				// first i decide on a number of values to give
				double ReLogSpacing = 0.02;
				double ReGuessValue = Math.Pow(10,ReLogSpacing * i);

				// once we have a suitable Re, we need to get a Be
				// value,
				// so we convert Re into mass flowrate
				//
				// Now unfortunately, for the first round of iteration,
				// we cannot guess Re from Be non iteratively.
				// We have to guess Be first, and then from that get Re.
				//
				// Problem is i don't know what a typical Be range should be!
				// Nor where the transtion region should be for any typical
				// graph.
				//
				// Well one method we can try is this:
				//
				// we have a guess Reynold's number value, which we then
				// feed into a pipe equation like churchill
				// from that, we can guess a Bejan number to go with
				// And from this Bejan number, we can guess the actual
				// Reynold's number value

				IFrictionFactor _frictionFactorObj = 
					new ChurchillFrictionFactor();

				// so bejan number is:
				// Be = 0.5 *Re^2 (f L/D  + K)
				//
				// Now herein lines a problem,
				// if we were to  use this method to guess,
				// we need a proper L/D ratio too.
				// To keep it once more in a similar order of
				// magnitude, i would rather have L be the average
				// of both branch lengths
				//
				// I assume the cubic spline would take care of 
				// any variation due to K, what i'm watching out for
				// more importantly is modelling the transition region
				// or interpolating it with sufficient points

				double bejanNumber = 1.0;

				BeValues.Add(bejanNumber);
			}

			this._interpolateBeFromRe = Interpolate.CubicSpline(
					BeValues,ReValues);

		}

		public MassFlow massFlowrateFromRe(double Re){

			// let's first get the cross 
			return new MassFlow(0.0, MassFlowUnit.KilogramPerSecond);
		}


		public override Length getComponentLength(){
			// first is that we get the definition object
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);

			// next we prepare a list of lengths
			List<Length> branchLengthList = new List<Length>();

			return new Length(0.0, LengthUnit.Meter);
		}

		public override Length getHydraulicDiameter(){
			return new Length(0.0, LengthUnit.Meter);
		}
    }
}
