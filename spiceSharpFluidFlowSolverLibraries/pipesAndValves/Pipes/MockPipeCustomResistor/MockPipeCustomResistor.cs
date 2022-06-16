﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.MockPipeCustomResistorBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor.
    /// </summary>
    /// <seealso cref="Component" />
    public class MockPipeCustomResistor : Component,
        IParameterized<BaseParameters>
    {

		// copied this from the nonlinear resistor, don't change!
		// but modified this to make it work without all that
		// autogenerated code
        public BaseParameters Parameters { get; set; } = new BaseParameters();

		// copied this from the nonlinear resistor, don't change!
        public MockPipeCustomResistor(string name, string nodeA, string nodeB) : base(name, 2)
        {
            Connect(nodeA, nodeB);
        }

		// Now, I want a constructor that can work without connecting
		// node A to B
        public MockPipeCustomResistor(string name) : base(name, 2){
			Console.WriteLine("\n Please Remember to call the Connect(inlet,outlet) method)\n");
			Console.WriteLine("ie ObjectName.Connect('inletName','outletName'); \n");
		}


		// copied this from the nonlinear resistor, don't change!
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors);
            if (simulation.UsesBehaviors<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}