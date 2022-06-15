using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MockPipeCustomResistorBehaviors
{
    public partial class BaseParameters : ParameterSet<BaseParameters>
    {
        public double A { get; set; } = 1.0e3;

        public double B { get; set; } = 1.0;
    }
}
