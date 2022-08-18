using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Entities
{
    public interface IHeatTransferFluidEntity : IFluidEntity
    {
		// IHeatTransferFluidEntity inherits from IFluidEntity
		// where IFluidEntity is an interface for isothermal fluids
		//
		// When heat transfer comes into play, we need to take into account
		// temperature variance, as such we need to have a list
		// of temperatures throughout the pipe to obtain the temperature
		// distribution at different areas or lengths of the pipe
		// this of course assumes segements are equally spaced
		// i assume that is enough for now

		IList<EngineeringUnits.Temperature> temperatureList { get; set; }

		int numberOfSegments { get; set; }

		IList<Length> lengthList { get; set; }

		// i also have overloads here with which to obtain fluid properties
		// given a temperature, this is much easier to unit test.
		Density getFluidDensity(EngineeringUnits.Temperature 
				fluidTemp);

		KinematicViscosity getFluidKinematicViscosity(EngineeringUnits.Temperature 
				fluidTemp);

		DynamicViscosity getFluidDynamicViscosity(EngineeringUnits.Temperature 
				fluidTemp);

		// for obtaining thermal conductivity i have two overloads
		// one obtains thermal conductivity at a given temperature (easy
		// to unit test)
		//
		// the other returns the averaged thermal conductivity based
		// on the temperature distributino
		ThermalConductivity getFluidThermalConductivity(
				EngineeringUnits.Temperature fluidTemp);

		ThermalConductivity getFluidThermalConductivity();

		// for obtaining heat capacity i also give two overloads,
		// one for the mean heat capacity in the pipe given a temperature 
		// distribution
		//
		// the other is explicitly for the user to define a temperature
		// and get a heat Capcity value
		//

		SpecificHeatCapacity getFluidHeatCapacity();

		SpecificHeatCapacity getFluidHeatCapacity(EngineeringUnits.Temperature 
				fluidTemp);

		// the same pattern i also use for prandtl number


		double getFluidPrandtl();

		double getFluidPrandtl(EngineeringUnits.Temperature 
				fluidTemp);
















    }
}
