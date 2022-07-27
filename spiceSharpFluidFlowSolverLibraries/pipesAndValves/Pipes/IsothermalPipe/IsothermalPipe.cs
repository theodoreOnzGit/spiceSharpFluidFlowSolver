﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.IsothermalPipeBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor.
    /// </summary>
    /// <seealso cref="Component" />
    public class IsothermalPipe : Component,
        IParameterized<BaseParameters>,
		SpiceSharp.Entities.IFluidEntity
    {

		// copied this from the nonlinear resistor, don't change!
		// but modified this to make it work without all that
		// autogenerated code
        public BaseParameters Parameters { get; set; } = new BaseParameters();

		// copied this from the nonlinear resistor, don't change!
        public IsothermalPipe(string name, string nodeA, string nodeB) : base(name, 2)
        {
            Connect(nodeA, nodeB);
        }

		// Now, I want a constructor that can work without connecting
		// node A to B
        public IsothermalPipe(string name) : base(name, 2){
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


		// these two additional methods fulfil the IFluidEntity 
		// interface
		//

		public Pressure getPressureDrop(MassFlow massFlowrate){
			return Parameters.getPressureDrop(massFlowrate);
		}

		public SpecificEnergy getKinematicPressureDrop(
				MassFlow massFlowrate){
			return Parameters.getKinematicPressureDrop(massFlowrate);
		}

		public MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop){
			// this supposes that a kinematicPressureDrop is supplied over this pipe
			// and we want to get the mass flowrate straightaway.
			
			return Parameters.getMassFlowRate(kinematicPressureDrop);
		}

		public MassFlow getMassFlowRate(Pressure dynamicPressureDrop){
			return Parameters.getMassFlowRate(dynamicPressureDrop);
		}

    }
}
