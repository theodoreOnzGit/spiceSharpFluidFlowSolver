using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.NonlinearResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="NonlinearResistor"/>
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public partial class BaseParameters : ParameterSet<BaseParameters>
    {
        public double A { get; set; } = 1.0e3;

        public double B { get; set; } = 1.0;
    }
}
