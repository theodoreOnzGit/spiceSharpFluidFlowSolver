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


		public IInterpolation _interpolateBeFromRe;
		public IInterpolation _interpolateNegativeBeFromNegativeRe;

		public void constructInterpolateBeFromRe(){
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
				double bejanNumber = 0.5 * 
					Math.Pow(ReGuessValue,2.0) *
					lengthToDiameter *
					darcyFrictionFactor;
				// once we have this we can add the bejan number
				BeValues.Add(bejanNumber);

				// after this we can get a mass flowrate from this bejan
				// number, Now this requires dimensionalising,
				// we need fluid kinematic viscosity or dynamic viscosity
				// or both!
				//
				// For a first iteration, kinematic viscosity will do
				// but it's likely we will need a representative value
				// of kinematic viscosity with which to scale it
				// ideally speaking this will be the kinematic viscosity of
				// the component before the 
				//
				// Neverthless, suppose that kinematic viscosity changes
				// throughout the pipe, how shall that be accounted for?
				// a kinematic viscosity that doesn't take into account
				// whatever is in the pipe may not make sense for the 
				// scaling kinematic viscosity
				//
				//
				// let me use the Bejan number to obtain a pressure drop
				Pressure samplePressureDrop = 
					this.getDynamicPressureDropFromBe(bejanNumber);

				MassFlow sampleMassFlow = 
					this.getMassFlowRate(samplePressureDrop);

				// now that we have mass flowrate, we can convert it into
				// a Reynold's number

				double ReynoldsNumberResult;
				ReynoldsNumberResult = this.ReFromMassFlowrate(sampleMassFlow);
				ReValues.Add(ReynoldsNumberResult);
			}
			
			// note then that the curve so far only covers the positive Re and 
			// Be region
			// we also need to add the negative values too

			IList<double> NegativeBeValues = new List<double>();
			IList<double> NegativeReValues = new List<double>();

			foreach (double bejanNumber in BeValues)
			{
				NegativeBeValues.Add(-bejanNumber);
				Pressure samplePressureDrop = 
					this.getDynamicPressureDropFromBe(-bejanNumber);

				MassFlow sampleMassFlow = 
					this.getMassFlowRate(samplePressureDrop);

				// now that we have mass flowrate, we can convert it into
				// a Reynold's number

				double ReynoldsNumberResult;
				ReynoldsNumberResult = this.ReFromMassFlowrate(sampleMassFlow);
				NegativeReValues.Add(ReynoldsNumberResult);
			}
			// so now i have two separate arrays, one Be list, one Re list
			// one negative Re List and one negative Be list
			// i'm going to merge the NegativeBeValues into the BeValues List


			this._interpolateBeFromRe = Interpolate.CubicSpline(
					ReValues,BeValues);
			this._interpolateNegativeBeFromNegativeRe = Interpolate.CubicSpline(
					NegativeReValues,NegativeBeValues);

		}

		public MassFlow massFlowrateFromRe(double Re){

			// Re = flowrate/A_XS * D_H/mu
			//
			// flowrate = Re * mu * A_XS /D_H

			MassFlow flowrate =
				this.getFluidKinematicViscosity() *
				this.getFluidDensity() *
				this.getXSArea() /
				this.getHydraulicDiameter();
			return flowrate;
		}

		public double ReFromMassFlowrate(MassFlow flowrate){

			// Re = flowrate/A_XS * D_H/mu
			//

			double Re = flowrate.As(MassFlowUnit.KilogramPerSecond) /
				this.getXSArea().As(AreaUnit.SquareMeter) *
				this.getHydraulicDiameter().As(LengthUnit.Meter) / 
				this.getFluidDynamicViscosity().
				As(DynamicViscosityUnit.PascalSecond);

			return Re;
		}

		public Pressure getDynamicPressureDropFromBe(double Be){
			// Be_D = \DeltaP D^2 / (mu * nu)
			// Delta P = Be_D *mu * nu / D^2
			
			Pressure resultPressure = 
				this.getFluidKinematicViscosity().Pow(2) *
				this.getFluidDensity() / 
				this.getHydraulicDiameter().Pow(2) *
				Be;
			return resultPressure;
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

		public override Area getXSArea(){
			// for cross-sectional area of the parallelSubcircuit
			// i will need to return a sum of all areas
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);
			// i'm only reading the Entities, not writing to it
			// so should be thread safe unless i set it.
			// Now i want to check  all the fluidEntities within the
			// fluidEntityCollection

			List<Area> areaList = new List<Area>();

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the area object
				Area _AreaForOneEntity =
					_fluidEntity.getXSArea();

				// convert it to m^2 just to be sure

				_AreaForOneEntity = _AreaForOneEntity.ToUnit(
						AreaUnit.SquareMeter);

				// lastly i add it to the areaListk
				areaList.Add(_AreaForOneEntity);

			}

			// once i'm done adding the areaList i can sum up all the values
			Area _totalArea = new Area(0.0, 
					AreaUnit.SquareMeter);
			foreach (Area _area in areaList)
			{
				_totalArea += _area;
			}


			return _totalArea;
		}

		public override Density getFluidDensity(){
			// to get fluid density, it is not simply an ensemble
			// average but the area weighted square root average
			//
			// let's first get the total area on standby

			Area _totalArea = this.getXSArea().ToUnit(AreaUnit.SquareMeter);


			// then let's get the fluidEntityCollection
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);

			// then i have a areaWeightedSquareRootDensityList
			List<double> areaWeightedSquareRootDensityList = 
				new List<double>();

			// let's start getting the areaWeightedSquareRootDensityList
			// by checking out each entity
			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the area object
				Area _AreaForOneEntity =
					_fluidEntity.getXSArea().ToUnit(
							AreaUnit.SquareMeter);
				
				// i also take the density object

				Density _DensityforOneEntity = 
					_fluidEntity.getFluidDensity().ToUnit(
							DensityUnit.KilogramPerCubicMeter);

				// so here im taking the density unit,
				// converting it into a double, and sqrt
				// times area of the entity
				// divide by total area
				double _areaWeightedSqrtDensity = 
					Math.Pow(_DensityforOneEntity.
							As(DensityUnit.KilogramPerCubicMeter),0.5)*
					_AreaForOneEntity.As(AreaUnit.SquareMeter)/ 
					_totalArea.As(AreaUnit.SquareMeter);


				areaWeightedSquareRootDensityList.Add(
						_areaWeightedSqrtDensity);
			}
			// after i have them added to a list, i need to
			// find the total,
			// so what i do is i just convert them all to double
			// and start adding them up
			//
			// the unit will be rather strange
			//
			// kg^0.5 / m^1.5
			// so i will call it, kg half per meter threehalves
			//
			// because variables don't like point or whatever


			double _areaWeightedSqrtDensityTotalKg_half_perMeterThreeHalves =
				0.0;

			foreach (double _areaWeightedSqrtDensity 
					in areaWeightedSquareRootDensityList)
			{
				_areaWeightedSqrtDensityTotalKg_half_perMeterThreeHalves +=
					_areaWeightedSqrtDensity;
			}

			// now that i've totalled summed the square root,
			// i can just create a new density unit to the environment

			double densityValue = 
				Math.Pow(_areaWeightedSqrtDensityTotalKg_half_perMeterThreeHalves,
						2.0);
			
			return new Density(densityValue, DensityUnit.KilogramPerCubicMeter);
		}


		public override KinematicViscosity getFluidKinematicViscosity(){
			// for parallel circuit setup, kinematic viscosity is 
			// averaged using ensemble average.
			// first i get the fluid entity collection
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);
			// i'm only reading the Entities, not writing to it
			// so should be thread safe unless i set it.
			// Now i want to check  all the fluidEntities within the
			// fluidEntityCollection

			List<KinematicViscosity> KinematicViscosityList =
			   new List<KinematicViscosity>();

			int numberOfBranches = 0;

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the kinematicViscosityObject
				KinematicViscosity _kinematicViscosityForOneEntity =
					_fluidEntity.getFluidKinematicViscosity();

				// convert it to m^2/s to  be sure

				_kinematicViscosityForOneEntity = _kinematicViscosityForOneEntity.
					ToUnit(KinematicViscosityUnit.SquareMeterPerSecond);

				// lastly i add it to the areaListk
				KinematicViscosityList.Add(_kinematicViscosityForOneEntity);

				numberOfBranches += 1;
			}

			// once i'm done adding the areaList i can sum up all the values
			KinematicViscosity _totalKinematicViscosity = 
				new KinematicViscosity(0.0, 
						KinematicViscosityUnit.SquareMeterPerSecond);
			foreach (KinematicViscosity _kinematicViscosityForOneEntity 
					in KinematicViscosityList)
			{
				_totalKinematicViscosity += _kinematicViscosityForOneEntity;
			}


			return _totalKinematicViscosity/numberOfBranches;
		}

		public DynamicViscosity getFluidDynamicViscosity(){
			return this.getFluidKinematicViscosity() * this.getFluidDensity();
		}
    }
}
