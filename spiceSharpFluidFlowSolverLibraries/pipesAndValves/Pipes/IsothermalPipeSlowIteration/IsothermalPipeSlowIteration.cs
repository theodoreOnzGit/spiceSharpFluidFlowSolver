﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.IsothermalPipeSlowIterationBehaviors;
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
    public class IsothermalPipeSlowIteration : Component,
        IParameterized<BaseParameters>,
		SpiceSharp.Entities.IFluidEntity
    {

		// copied this from the nonlinear resistor, don't change!
		// but modified this to make it work without all that
		// autogenerated code
        public BaseParameters Parameters { get; set; } = new BaseParameters();

		// copied this from the nonlinear resistor, don't change!
        public IsothermalPipeSlowIteration(string name, string nodeA, string nodeB) : base(name, 2)
        {
            Connect(nodeA, nodeB);
        }

		// Now, I want a constructor that can work without connecting
		// node A to B
        public IsothermalPipeSlowIteration(string name) : base(name, 2){
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
		// Warning: none of these methods are thread safe. 
		// only one method should be used at one time per instance

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

		public Length getComponentLength(){
			return Parameters.pipeLength;
		}

		public Length getHydraulicDiameter(){
			return Parameters.hydraulicDiameter;
		}

		public Area getXSArea(){
			return Parameters.crossSectionalArea();
		}

		public Density getFluidDensity(){
			return Parameters.fluidDesnity();
		}

		public KinematicViscosity getFluidKinematicViscosity(){
			return Parameters.fluidKinViscosity;
		}

		public IList<EngineeringUnits.Temperature> temperatureList { 
			get{
				throw new NotImplementedException();
			}
			set{
				throw new NotImplementedException();
			}
		}

		public DynamicViscosity getFluidDynamicViscosity(){
			return Parameters.fluidViscosity;
		}

		public Pressure getHydrostaticPressureChange(){
			return Parameters.hydrostaticPressureChange();
		}

		public Length getZ(){
			Length heightChange =
				Parameters.pipeLength * Math.Sin(Parameters.inclineAngle.As(
							AngleUnit.Radian));
			return heightChange;
		}


		public (Length, Length, Length) getCoordinateChange(){
			throw new NotImplementedException();
		}

		public double getBejanFromPressureDrop(Pressure pressureDrop){
			double Be;
			Be = pressureDrop.As(PressureUnit.Pascal);
			Be *= Math.Pow(this.getHydraulicDiameter().As(LengthUnit.Meter
						),2.0);
			Be /= this.getFluidDynamicViscosity().As(DynamicViscosityUnit.
					PascalSecond);
			Be /= this.getFluidKinematicViscosity().As(KinematicViscosityUnit.
					SquareMeterPerSecond);

			return Be;
		}

		public Pressure getDynamicPressureDropFromBe(double Be){

			Pressure finalPressure =  
				this.getFluidDynamicViscosity().
				ToUnit(DynamicViscosityUnit.PascalSecond) *
				this.getFluidKinematicViscosity().
				ToUnit(KinematicViscosityUnit.SquareMeterPerSecond) /
				this.getHydraulicDiameter().
				ToUnit(LengthUnit.Meter) / 
				this.getHydraulicDiameter().
				ToUnit(LengthUnit.Meter);

			finalPressure *= Be;

			return finalPressure;

		}

		public double getReFromMassFlow(MassFlow flowrate){
			return Parameters.getReynoldsNumber(flowrate);
		}

		public MassFlow getMassFlowRateFromRe(double Re){
			MassFlow finalMassFlow;
			finalMassFlow = this.getXSArea().ToUnit(AreaUnit.SquareMeter) *
				this.getFluidDynamicViscosity().
				ToUnit(DynamicViscosityUnit.PascalSecond) /
				this.getHydraulicDiameter().
				ToUnit(LengthUnit.Meter) *
				Re;

			return finalMassFlow;
		}
    }
}
