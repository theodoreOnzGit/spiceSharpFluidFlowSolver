using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components.BasePipeBehaviors
{
    public partial class BaseParameters : ParameterSet<BaseParameters>
    {
        public double A { get; set; } = 1.0e3;

        public double B { get; set; } = 1.0;

		public double roughnessRatio { get; set; } = 0.0;

		public double lengthToDiameterRatio { get; set; } = 4.0;

		// now we have the dimensioned units for pipe
		// this is assumed to be circular

		public Length hydraulicDiameter { get; set; } = 
			new Length(1.0,LengthUnit.SI);

		public Length pipeLength { get; set; } = 
			new Length(10.0,LengthUnit.SI);

		// carbon steel surface roughness used as default
		public Length absoluteRoughness { get; set; } =
			new Length (0.15, LengthUnit.Millimeter);

		public Area crossSectionalArea(){
			Area finalResult;
			finalResult = this.hydraulicDiameter.Pow(2)/4*Math.PI;
			return finalResult;
		}
		

		// and also fluid properties
		// water at 18C used as default
		// https://www.engineeringtoolbox.com/water-dynamic-kinematic-viscosity-d_596.html

		public KinematicViscosity fluidKinViscosity { get; set; } =
			new KinematicViscosity(1.0533, KinematicViscosityUnit.Centistokes);

		public DynamicViscosity fluidViscosity { get; set; } =
			new DynamicViscosity(1.0518, DynamicViscosityUnit.Centipoise);



    }
}
