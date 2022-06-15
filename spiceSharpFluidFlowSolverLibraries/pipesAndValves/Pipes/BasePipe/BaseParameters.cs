using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BasePipeBehaviors
{
    public partial class BaseParameters : ParameterSet<BaseParameters>
    {
        public double A { get; set; } = 1.0e3;

        public double B { get; set; } = 1.0;

		public double roughnessRatio { get; set; } = 0.0;

		public double lengthToDiameterRatio { get; set; } = 4.0;
    }
}
