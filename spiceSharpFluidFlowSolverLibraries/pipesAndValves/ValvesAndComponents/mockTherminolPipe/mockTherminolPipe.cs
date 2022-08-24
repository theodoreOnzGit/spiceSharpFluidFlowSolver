﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.mockTherminolPipeBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using EngineeringUnits;
using EngineeringUnits.Units;
using SharpFluids;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor.
    /// </summary>
    /// <seealso cref="Component" />
    public class mockTherminolPipe : TherminolPipe,
        IParameterized<BaseParameters>
    {

		// copied this from the nonlinear resistor, don't change!
		// but modified this to make it work without all that
		// autogenerated code
        public BaseParameters Parameters { get; set; } = new BaseParameters();

		// copied this from the nonlinear resistor, don't change!
        public mockTherminolPipe(string name, string nodeA, string nodeB) 
			: base(name, nodeA, nodeB)
        {
        }

		// Now, I want a constructor that can work without connecting
		// node A to B
        public mockTherminolPipe(string name) : base(name){
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

		public override ThermalConductivity getFluidThermalConductivity(){
			throw new NotImplementedException();
		}

		public override SpecificHeatCapacity getFluidHeatCapacity(){
			throw new NotImplementedException();
		}

		public override DynamicViscosity getFluidDynamicViscosity(){
			throw new NotImplementedException();
		}

		public override double getFluidPrandtl(){
			throw new NotImplementedException();
		}

		public override IList<EngineeringUnits.Temperature> 
			temperatureList { get; set; } = 
			new List<EngineeringUnits.Temperature>();


		public override void setHydraulicDiameters(){
			this.entranceHydraulicDiameter = new Length(2.39e-2, LengthUnit.Meter);
			this.exitHydraulicDiameter = new Length(2.39e-2, LengthUnit.Meter);
		}

		public override Length getSurfaceRoughness(){
			// using drawn copper aboslute roughness in mm
			// from engineeringtoolbox.com
			Length absoluteRoughness = new Length(0.002, LengthUnit.Meter);
			return absoluteRoughness;
		}

		public override void setComponentLength(){
			this.componentLength= new Length(0.5, LengthUnit.Meter);
		}

		public override double getFormLossCoefficientK(){
			return 0.0;
		}

		public override KinematicViscosity getFluidKinematicViscosity(){
			throw new NotImplementedException();
		}

		public override Density getFluidDensity(){
			throw new NotImplementedException();
		}

    }
}
