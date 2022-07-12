using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components.IsothermalPipeBehaviors
{
    public partial class BaseParameters : 
		ParameterSet<BaseParameters>
    {

		// now we have the dimensioned units for pipe
		// this is assumed to be circular

		public Length hydraulicDiameter { get; set; } = 
			new Length(1.0,LengthUnit.SI);

		public Length pipeLength { get; set; } = 
			new Length(10.0,LengthUnit.SI);

		// next we also have angles as well

		public Angle inclineAngle { get; set; } =
			new Angle(0.0, AngleUnit.Degree);

		// carbon steel surface roughness used as default
		public Length absoluteRoughness { get; set; } =
			new Length (0.15, LengthUnit.Millimeter);

		// derived quantites and ratios

		public Area crossSectionalArea(){
			Area finalResult;
			finalResult = this.hydraulicDiameter.Pow(2)/4*Math.PI;
			return finalResult;
		}

		public double roughnessRatio(){
			return absoluteRoughness.As(LengthUnit.SI)/
				hydraulicDiameter.As(LengthUnit.SI);
		}

		public double lengthToDiameter(){
			return pipeLength.As(LengthUnit.SI)/
				hydraulicDiameter.As(LengthUnit.SI);
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
