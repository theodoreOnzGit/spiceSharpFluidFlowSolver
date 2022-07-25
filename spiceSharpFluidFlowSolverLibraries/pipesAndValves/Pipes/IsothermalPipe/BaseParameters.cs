using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;
using EngineeringUnits;
using EngineeringUnits.Units;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace SpiceSharp.Components.IsothermalPipeBehaviors
{
    public partial class BaseParameters : 
		ParameterSet<BaseParameters>
    {

		// now we have the dimensioned units for pipe
		// this is assumed to be circular

		public Length hydraulicDiameter { get; set; } = 
			new Length(1.0,LengthUnit.Meter);

		public Length pipeLength { get; set; } = 
			new Length(10.0,LengthUnit.Meter);

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
			return absoluteRoughness.As(LengthUnit.Meter)/
				hydraulicDiameter.As(LengthUnit.Meter);
		}

		public double lengthToDiameter(){
			return pipeLength.As(LengthUnit.Meter)/
				hydraulicDiameter.As(LengthUnit.Meter);
		}
		


		// and also fluid properties
		// water at 18C used as default
		// https://www.engineeringtoolbox.com/water-dynamic-kinematic-viscosity-d_596.html

		public KinematicViscosity fluidKinViscosity { get; set; } =
			new KinematicViscosity(1.0533, KinematicViscosityUnit.Centistokes);

		public DynamicViscosity fluidViscosity { get; set; } =
			new DynamicViscosity(1.0518, DynamicViscosityUnit.Centipoise);

		public IFrictionFactorJacobian jacobianObject =
			new StabilisedChurchillJacobian();
		
		// we can get dynamic pressure drop from mass flowrate as follows
		// (1) convert mass flowrate to Reynolds number
		// (2) convert Reynolds number to Bejan number
		// (3) convert Bejan number back to pressure drop


		public double getReynoldsNumber(MassFlow massFlowrate){
			// the reynold's number here is based off mass flowrate
			// massflowrate/XSArea * hydraulicDiameter / dynamicViscosity
			double ReynoldsNumber;
			ReynoldsNumber = massFlowrate.As(MassFlowUnit.KilogramPerSecond);
			ReynoldsNumber /= this.crossSectionalArea().As(AreaUnit.SquareMeter);
			ReynoldsNumber *= this.hydraulicDiameter.As(LengthUnit.Meter);
			ReynoldsNumber /= this.fluidViscosity.As(
					DynamicViscosityUnit.PascalSecond);

			return ReynoldsNumber;
		}

		// this function is here to return a Bejan number
		// this is based on Be_D not Be_L
		// ie Be = deltaP * D^2/nu^2
		// i use the formula
		// (f_darcy L/D + K )Re^2 = 2 Be_D
		// i assume K is exactly zero here
		// so no need to include that

		public double getBejanNumber(MassFlow massFlowrate){

			// now i want to develop a way to deal with negative numbers
			// by default, isNegativeMassFlow is false
			bool isNegativeMassFlow = false;

			if (massFlowrate.As(MassFlowUnit.SI) < 0.0)
			{
				// if mass flowrate is less than 0,
				// change the boolean isNegativeMassFlow to true
				// and make the massFlowrate positive
				isNegativeMassFlow = true;
				massFlowrate *= -1.0;
			}

			double Re = this.getReynoldsNumber(massFlowrate);

			double darcyFrictionFactorReSq =
				this.constantRoughnessFanningReSq(Re)
				*4.0;

			double BejanNumber;
			BejanNumber = 0.5;
			BejanNumber *= darcyFrictionFactorReSq;
			BejanNumber *= this.lengthToDiameter();

			if(isNegativeMassFlow){
				// if i have a negative mass flowrate value,
				// i will return a negative Bejan Number
				return BejanNumber * -1.0;
			}
			return BejanNumber;

		}

		private double constantRoughnessFanningReSq(double Re){

			// now the graph in the negative x direction should be a 
			// reflection of the graph in positive x direction along
			// the y axis. So this is using absolute value of Re

			
			Re = Math.Abs(Re);

			// the fanning friction factor function (this.fanning)
			// can return Re values for various values in turbulent as
			// well as laminar region
			//
			// However, if Re is close to zero, 
			// the function is not well behaved
			//
			// since we are returning f*Re^2
			// we can use the laminar region fanning friction factor
			// which is 16/Re
			// for lower Re eg. Re<1
			// However, we also note that it's quite computationally cheap
			// in that you only need to perform one calculation
			// Hence, it's quite advantageous to let it take more Reynold's 
			// numbers
			// so for most of the laminar regime, it is good to use the 
			// 16/Re formula
			//
			// We should note however that for a piecewise function
			// there is some discontinuity between the two functions
			// ie the churchill and the Pousille function
			//
			// While this is a concern, let's ignore it for now
			// and fix the problem if it crops up.
			//
			// So if Re>1800 we return the traditional fanning formula

			double transitionPoint = 1800.0;

			if (Re > transitionPoint)
			{
				double fanningReSq = jacobianObject.fanning(
						Re, this.roughnessRatio())*
					Math.Pow(Re,2.0);

				return fanningReSq;
			}

			// otherwise we return 16/Re*Re^2 or 16*Re
			//
			IInterpolation _linear;

			IList<double> xValues = new List<double>();
			IList<double> yValues = new List<double>();
			xValues.Add(0.0);
			xValues.Add(transitionPoint);

			yValues.Add(0.0);
			yValues.Add(jacobianObject.fanning(transitionPoint,
						this.roughnessRatio())*
					Math.Pow(transitionPoint,2.0));

			_linear = Interpolate.Linear(xValues,yValues);


			return _linear.Interpolate(Re);
			
			// my only concern here is a potential problem if the root is exactly at
			// Re = 1800
		}

		// this is a function to get the pressure drop
		// given a mass flowrate
		// we use the formula
		// Be_D = Delta P * D^2 / (mu*nu)
		public Pressure getPressureDrop(MassFlow massFlowrate){

			double BejanNumber = this.getBejanNumber(massFlowrate);

			Pressure pressureDrop = this.fluidKinViscosity *
				this.fluidViscosity /
				this.hydraulicDiameter /
				this.hydraulicDiameter;

			pressureDrop *= BejanNumber;
			pressureDrop = pressureDrop.ToUnit(
					PressureUnit.Pascal);

			return pressureDrop;
			

		}

		public SpecificEnergy getKinematicPressureDrop(
				MassFlow massFlowrate){

			double BejanNumber = this.getBejanNumber(massFlowrate);

			SpecificEnergy pressureDrop = this.fluidKinViscosity *
				this.fluidKinViscosity /
				this.hydraulicDiameter /
				this.hydraulicDiameter;

			pressureDrop *= BejanNumber;
			pressureDrop = pressureDrop.ToUnit(
					SpecificEnergyUnit.JoulePerKilogram);

			return pressureDrop;
			

		}

    }
}
