using EngineeringUnits;
using EngineeringUnits.Units;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    public abstract class FluidEntity : Entity,
        IEntity, IFluidEntity
    {

        /// <summary>
        /// Constructor for fluid entity, 
		/// doesn't do anything but call the base constructor
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected FluidEntity(string name) : base(name)
        {
        }

		public abstract Pressure getPressureDrop(MassFlow flowrate);

		public abstract SpecificEnergy getKinematicPressureDrop(MassFlow flowrate);

		public abstract MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop);

		public abstract MassFlow getMassFlowRate(Pressure dynamicPressureDrop);

		public abstract Length getComponentLength();

		public abstract Length getHydraulicDiameter();

		public abstract Area getXSArea();

		public abstract Density getFluidDensity();

		public abstract KinematicViscosity getFluidKinematicViscosity();

		public abstract IList<EngineeringUnits.Temperature> temperatureList { get; set; }

		public abstract DynamicViscosity getFluidDynamicViscosity();

		public abstract Pressure getHydrostaticPressureChange();

		public abstract Length getZ();

		public abstract (Length, Length, Length) getCoordinateChange();

		public abstract double getBejanFromPressureDrop(
				Pressure pressureDrop);

		public abstract Pressure getDynamicPressureDropFromBe(double Be);

		public abstract double getReFromMassFlow(MassFlow flowrate);

		public abstract MassFlow getMassFlowRateFromRe(double Re);
    }

    /// <summary>
    /// Base class for any circuit object that can take part in simulations.
    /// This variant also defines a cloneable parameter set.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    public abstract class FluidEntity<P> : Entity<P>, IFluidEntity
        where P : IParameterSet, ICloneable<P>, new()
    {

        /// <summary>
        /// Just calls the base constructor
        /// </summary>
        /// <param name="name">The name.</param>
        protected FluidEntity(string name)
            : base(name)
        {
        }

		public abstract Pressure getPressureDrop(MassFlow flowrate);

		public abstract SpecificEnergy getKinematicPressureDrop(MassFlow flowrate);

		public abstract MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop);

		public abstract MassFlow getMassFlowRate(Pressure dynamicPressureDrop);

		public abstract Length getComponentLength();

		public abstract Length getHydraulicDiameter();

		public abstract Area getXSArea();

		public abstract Density getFluidDensity();

		public abstract KinematicViscosity getFluidKinematicViscosity();

		public abstract IList<EngineeringUnits.Temperature> temperatureList { get; set; }

		public abstract DynamicViscosity getFluidDynamicViscosity();

		public abstract Pressure getHydrostaticPressureChange();

		public abstract Length getZ();

		public abstract (Length, Length, Length) getCoordinateChange();

		public abstract double getBejanFromPressureDrop(
				Pressure pressureDrop);

		public abstract Pressure getDynamicPressureDropFromBe(double Be);

		public abstract double getReFromMassFlow(MassFlow flowrate);

		public abstract MassFlow getMassFlowRateFromRe(double Re);
    }
}
