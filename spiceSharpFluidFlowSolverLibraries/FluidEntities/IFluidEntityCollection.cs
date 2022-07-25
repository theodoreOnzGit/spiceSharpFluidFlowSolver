using System;
using System.Collections.Generic;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Entities
{
    public interface IFluidEntityCollection : 
		IEntityCollection
    {
		// here we have two methods to return both
		// dynamic and kinematic pressure given a flowrate
		Pressure getPressureDrop(MassFlow flowrate);

		SpecificEnergy getKinematicPressureDrop(MassFlow flowrate);

		Dictionary<string, IFluidEntity> _fluidEntities { get; set; }

		// now suppose i have a full collection of IFluidEntities, 
		// i will now want to be able to calculate the mass flowrate as well
		// if i supply a kinematicPressureDrop or dynamicPressureDrop

		MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop);

		MassFlow getMassFlowRate(Pressure dynamicPressureDrop);
    }
}
