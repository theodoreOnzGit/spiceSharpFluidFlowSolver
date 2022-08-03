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
    public interface IFluidParallelSubCircuit     
	{
		// i want to have a function which checks the elevations of each
		// branch to make sure they are equal
		// ie gz is the same, which means i need a method in each
		// of my IFluidEntities to get elevation or head
		void checkBranchElevation();
    }
}
