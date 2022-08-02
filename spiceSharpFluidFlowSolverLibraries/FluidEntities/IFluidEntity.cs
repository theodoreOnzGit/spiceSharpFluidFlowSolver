using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Entities
{
    public interface IFluidEntity : IEntity
    {
		// here we have two methods to return both
		// dynamic and kinematic pressure given a flowrate
		Pressure getPressureDrop(MassFlow flowrate);

		SpecificEnergy getKinematicPressureDrop(MassFlow flowrate);

		MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop);

		MassFlow getMassFlowRate(Pressure dynamicPressureDrop);

		Length getComponentLength();

		Length getHydraulicDiameter();

		Area getXSArea();

		Density getFluidDensity();

		KinematicViscosity getFluidKinematicViscosity();
    }
}
