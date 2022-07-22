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
    }
}
