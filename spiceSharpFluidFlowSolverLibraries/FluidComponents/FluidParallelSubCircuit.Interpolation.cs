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
				// so i assume K = 0
				//
				// more importantly is modelling the transition region
				// or interpolating it with sufficient points
				// to get L/D ratio, we need the average branch lengths
				// and average hydraulic diameters

				double bejanNumber = 1.0;
				double lengthToDiameter = this.getComponentLength().
					As(LengthUnit.Meter) / 
					this.getHydraulicDiameter().
					As(LengthUnit.Meter);

				// my roughness ratio here is guessed based on 
				// assuming cast iron pipes, just a guestimation
				// so not so important

				Length absoluteRoughness = new Length(
						0.15, LengthUnit.Millimeter);


				double roughnessRatio = absoluteRoughness.As(LengthUnit.Meter)/ 
					this.getHydraulicDiameter().As(LengthUnit.Meter);


				double darcyFrictionFactor = _frictionFactorObj.
					darcy(ReGuessValue, roughnessRatio);

				// i shall now shove these values in to obtain my Bejan number
				// Be_d = 0.5*Re(guess)^2 *f * L/D
				double BejanNumber = 0.5 * 
					Math.Pow(ReGuessValue,2.0) *
					lengthToDiameter *
					darcyFrictionFactor;
				// once we have this we can add the bejan number
				BeValues.Add(bejanNumber);

				// after this we can get a mass flowrate from this bejan
				// number
			}

			this._interpolateBeFromRe = Interpolate.CubicSpline(
					ReValues,BeValues);

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

			// i also prepare the number of branches within this 
			// fluidEntityCollection
			//
			int numberOfBranches = 0;

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the length object
				//

				Length componentLength = 
					_fluidEntity.getComponentLength();

				// i add it to the list

				branchLengthList.Add(componentLength);

				// last step is to increase number of branches by 1

				numberOfBranches += 1;
			}

			if (numberOfBranches < 1)
			{

				string errorMsg = "you didn't add any branches to \n";
				errorMsg += "the FluidParallelSubCircuit \n";
				throw new InvalidOperationException(errorMsg);
			}

			Length _totalBranchLengths = new Length(0.0, LengthUnit.Meter);

			foreach (var _branchLength in branchLengthList)
			{
				_totalBranchLengths += _branchLength;
			}

			
			Length _averageBranchLength = _totalBranchLengths / 
				numberOfBranches;

			return _averageBranchLength;
		}

		public override Length getHydraulicDiameter(){
			// this method returns the average hydraulic diameter of the branches

			// first is that we get the definition object
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);

			// next we prepare a list of hydraulic Diameters
			List<Length> hydraulicDiameterList = new List<Length>();

			// i also prepare the number of branches within this 
			// fluidEntityCollection
			//
			int numberOfBranches = 0;

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the length object
				//

				Length hydraulicDiameter = 
					_fluidEntity.getHydraulicDiameter();

				// i add it to the list

				hydraulicDiameterList.Add(hydraulicDiameter);

				// last step is to increase number of branches by 1

				numberOfBranches += 1;
			}

			if (numberOfBranches < 1)
			{

				string errorMsg = "you didn't add any branches to \n";
				errorMsg += "the FluidParallelSubCircuit \n";
				throw new InvalidOperationException(errorMsg);
			}

			Length _totalBranchHydraulicDiameters = 
				new Length(0.0, LengthUnit.Meter);

			foreach (var _branchHydraulicDiameter in hydraulicDiameterList)
			{
				_totalBranchHydraulicDiameters += _branchHydraulicDiameter;
			}

			
			Length _averageHydraulicDiameter = _totalBranchHydraulicDiameters / 
				numberOfBranches;

			return _averageHydraulicDiameter;
		}

    }
}
