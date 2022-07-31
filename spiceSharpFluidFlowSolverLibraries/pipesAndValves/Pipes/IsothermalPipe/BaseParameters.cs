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
		// this is mainly to calculate hydrostatic pressure increase or
		// decrease

		public Angle inclineAngle { get; set; } =
			new Angle(0.0, AngleUnit.Degree);

		public Pressure hydrostaticPressureChange(){

			// this is the change in hydrostatic pressure 
			// ie pressure of pipe exit - pressure of pipe entrance
			Pressure hydrostaticPressureChange;


			hydrostaticPressureChange = 
				this.hydrostaticKinematicPressureChange()*
				this.fluidDesnity();

			hydrostaticPressureChange = 
				hydrostaticPressureChange.ToUnit(
						PressureUnit.Pascal);

			return hydrostaticPressureChange;
		}

		public SpecificEnergy hydrostaticKinematicPressureChange(){

			// this is the change in hydrostatic pressure 
			// ie pressure of pipe exit - pressure of pipe entrance
			SpecificEnergy hydrostaticKinematicPressureChange;

			Length heightChange =
				this.pipeLength * Math.Sin(this.inclineAngle.As(
							AngleUnit.Radian));

			// for gravity let me initiate a constants class

			Acceleration _standardGravity =
				new Acceleration(9.81, AccelerationUnit.MeterPerSecondSquared);

			// now kinematic pressure change is just gz

			hydrostaticKinematicPressureChange = 
				heightChange * _standardGravity;

			hydrostaticKinematicPressureChange = 
				hydrostaticKinematicPressureChange.ToUnit(
						SpecificEnergyUnit.JoulePerKilogram);

			return hydrostaticKinematicPressureChange;


		}
		

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

		public Density fluidDesnity(){
			// i find the density here by comparing
			// kinematic viscosity to dynamic viscosity ratios
			// mu = rho* nu
			// rho = mu/nu

			DynamicViscosity fluidViscosity = 
				this.fluidViscosity;
			
			fluidViscosity = fluidViscosity.ToUnit(DynamicViscosityUnit.
					PascalSecond);

			KinematicViscosity fluidKinViscosity =
				this.fluidKinViscosity;

			fluidKinViscosity = fluidKinViscosity.ToUnit(KinematicViscosityUnit.
					SquareMeterPerSecond);

			Density fluidDesnity = fluidViscosity/fluidKinViscosity;
			fluidDesnity = fluidDesnity.ToUnit(
					DensityUnit.KilogramPerCubicMeter);

			return fluidDesnity;
		}

		// constructors
		//

		public BaseParameters(){
			// this constructor is here to construct interpolation objects
			this.constructInterpolateReFromBe();
		}


		IInterpolation _interpolateReFromBe;

		public void constructInterpolateReFromBe(){
			// first let me get a set of about 1000 values
			//

			IList<double> ReValues = new List<double>();
			IList<double> BeValues = new List<double>();
			for (int i = 0; i < 1000; i++)
			{
				// first i decide on a number of values to give
				double ReSpacing = 100.0;
				double ReValue = ReSpacing * i;
				ReValues.Add(ReValue);

				// then i decide that okay, let me get my Bejan numbers
				// from Re
				// Be_D = 0.5 * (f_darcy * L/D + K) Re^2
				// here, we don't exactly consider K just yet
				// so we just use
				//
				// Be_D = 0.5* (f_darcy* L/D) Re^2,
				// what we need here though is L/D and roughness ratio
				// besides Re
				double bejanNumber;
				double darcyFrictionFactorReSq =
					this.constantRoughnessFanningReSq(ReValue)
					*4.0;
				bejanNumber = 0.5* darcyFrictionFactorReSq *
					this.lengthToDiameter();
				BeValues.Add(bejanNumber);
			}

			this._interpolateReFromBe = Interpolate.Linear(
					BeValues,ReValues);

		}

		//
		// i have changed this to a function returning new instances
		// of StabilisedChurchillJacobian in order to avoid race
		// conditions
		public IFrictionFactorJacobian jacobianObject(){
			return new StabilisedChurchillJacobian();
		}
		
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

			if (massFlowrate.As(MassFlowUnit.KilogramPerSecond) < 0.0)
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
				double fanningReSq = jacobianObject().fanning(
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
			yValues.Add(jacobianObject().fanning(transitionPoint,
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
		// we don't include hydrostatic pressure changes here
		public Pressure getPressureDrop(MassFlow massFlowrate){

			double BejanNumber = this.getBejanNumber(massFlowrate);

			KinematicViscosity fluidKinViscosity;
			fluidKinViscosity = this.fluidKinViscosity.ToUnit(
					KinematicViscosityUnit.SquareMeterPerSecond);

			DynamicViscosity fluidViscosity;
			fluidViscosity = this.fluidViscosity.ToUnit(
					DynamicViscosityUnit.PascalSecond);

			Pressure pressureDrop = fluidKinViscosity *
				fluidViscosity /
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
			
			KinematicViscosity fluidKinViscosity;
			fluidKinViscosity = this.fluidKinViscosity.ToUnit(
					KinematicViscosityUnit.SquareMeterPerSecond);

			SpecificEnergy pressureDrop = fluidKinViscosity *
				fluidKinViscosity /
				this.hydraulicDiameter /
				this.hydraulicDiameter;

			pressureDrop *= BejanNumber;
			pressureDrop = pressureDrop.ToUnit(
					SpecificEnergyUnit.JoulePerKilogram);

			return pressureDrop;
			

		}

		// here are methods to get MassFlowrate
		//

		public MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop){
			// this supposes that a kinematicPressureDrop is supplied over this pipe
			// and we want to get the mass flowrate straightaway.
			//

			// note: it's not thread safe to run multiple instances
			// of getMassFlowrate since i will be manipulating variables multiple
			// times
			
			// first let's define a function to help us
			// get a double of kinematicPressureDrop from
			// the double of massFlowrate
			// numbers must be in double format so as to
			// make it compatible with MathNet FindRoots.OfFunction
			// rootfinder
			//
			// I think an upper value of 1e12 kg/s will suffice
			//
			// so this function will take in a massFlowrate
			// and iterate till the desired kinematic pressure dorp is achieved
			//
		    
			// before i start calculations, i will need to adjust for hydrostatic pressure
			//
			kinematicPressureDrop -= this.hydrostaticKinematicPressureChange();
			// first i'll need to see if this value is negative 

			bool isNegativePressure = false;

			if (kinematicPressureDrop.As(SpecificEnergyUnit.SI) < 0.0)
			{
				isNegativePressure = true;
				kinematicPressureDrop *= -1.0;
			}
			// next i want to see if pressure drop is zero
			// i will automatically just return mass flowrate of zero

			if (kinematicPressureDrop.As(SpecificEnergyUnit.SI) == 0.0){
				return new MassFlow(0.0, MassFlowUnit.KilogramPerSecond);
			}
			// now if the Bejan number is low enough, we can just use
			// interpolation done from the constructor objects
			// let's get Bejan number first


			double getBejanFromKinematicPressureDrop(SpecificEnergy 
					kinematicPressureDrop){
				// Be_D = kinPressureDrop * D^2/nu^2
				// D = hydraulicDiameter
				// nu = kinematicViscosity
				double Be_D;

				double nuSqared = Math.Pow(this.fluidKinViscosity.As( 
							KinematicViscosityUnit.SquareMeterPerSecond)
						,2.0);

				double Dsquared = Math.Pow(this.hydraulicDiameter.As(
							LengthUnit.Meter)
						,2.0);


				Be_D = kinematicPressureDrop.As(SpecificEnergyUnit.
						JoulePerKilogram) * Dsquared / nuSqared;

				return Be_D;

			}

			double Be_D;
			Be_D = getBejanFromKinematicPressureDrop(kinematicPressureDrop);

			if(Be_D < 100000){
				// if Be_D is sufficiently small, within linear range,
				// we can interpolate it rather than go about iterating our
				// way to an answer
				double Re;
				Re = this._interpolateReFromBe.Interpolate(Be_D);


				MassFlow massFlowrateFromRe(double Re){

					// Re = massflow/
					MassFlow flowrate =
						this.crossSectionalArea()/
						this.hydraulicDiameter*
						this.fluidViscosity*
						Re;

					return flowrate.ToUnit(MassFlowUnit.
							KilogramPerSecond);

				}

				return massFlowrateFromRe(Re);
				
			}

			this.kinematicPressureDropValJoulePerKg =
				kinematicPressureDrop.As(SpecificEnergyUnit.
						JoulePerKilogram);

			double pressureDropRoot(double massFlowValueKgPerS){

				// so i have a reference kinematic presureDrop value
				double kinematicPressureDropValJoulePerKg = 
					this.kinematicPressureDropValJoulePerKg;

				// and then i also have a iterated pressureDrop value

				double iteratedPressureDropValJoulePerKg;

				MassFlow massFlowrate;
				massFlowrate = new MassFlow(massFlowValueKgPerS,
						MassFlowUnit.KilogramPerSecond);


				SpecificEnergy kinematicPressureDrop;
				kinematicPressureDrop = 
					this.getKinematicPressureDrop(massFlowrate);

				iteratedPressureDropValJoulePerKg =
					kinematicPressureDrop.As(SpecificEnergyUnit.
							JoulePerKilogram);

				// so this is the function, to have iterated pressure drop
				// equal the kinematic pressure drop,
				// set the value to zero
				double functionValue =
					iteratedPressureDropValJoulePerKg -
					kinematicPressureDropValJoulePerKg;


				return functionValue;
			}

			// here i use MathNet's FindRoots function to get the mass flowrate
			double massFlowValueKgPerS;
			massFlowValueKgPerS = FindRoots.OfFunction(pressureDropRoot,
					-1e12,1e12);

			MassFlow massFlowrate;
			massFlowrate = new MassFlow(massFlowValueKgPerS,
					MassFlowUnit.KilogramPerSecond);

			// after i'm done, do some cleanup operations
			this.kinematicPressureDropValJoulePerKg = 0.0;


			// and finally let me return the mass flowrate
				if (isNegativePressure)
				{
					return massFlowrate * -1.0;
				}
			return massFlowrate;

		
		}

		public double kinematicPressureDropValJoulePerKg = 0.0;

		public MassFlow getMassFlowRate(Pressure dynamicPressureDrop){
			// before we start anything
			// we adjust for hydrostatic pressure

			dynamicPressureDrop -= this.hydrostaticPressureChange();
			// first let's check for negative pressure values
			// ie reverse direction
			bool isNegativePressure = false;

			if (dynamicPressureDrop.As(PressureUnit.SI) < 0.0)
			{
				isNegativePressure = true;
				dynamicPressureDrop *= -1.0;
			}

			// next i want to see if pressure drop is zero
			// i will automatically just return mass flowrate of zero

			if (dynamicPressureDrop.As(PressureUnit.SI) == 0.0){
				return new MassFlow(0.0, MassFlowUnit.KilogramPerSecond);
			}

			this.dynamicPressureDropValuePascal = 
				dynamicPressureDrop.As(PressureUnit.
						Pascal);

			double pressureDropRoot(double massFlowValueKgPerS){

				double dynamicPressureDropValuePascal 
					= this.dynamicPressureDropValuePascal;

				// let's do the iterated pressureDropValue
				double iteratedPressureDropValPascal;

				// so i'll have a mass flowrate
				// and iterate a pressure drop out
				MassFlow massFlowrate;
				massFlowrate = new MassFlow(massFlowValueKgPerS,
						MassFlowUnit.KilogramPerSecond);


				Pressure pressureDrop 
					= this.getPressureDrop(massFlowrate);

				pressureDrop = pressureDrop.ToUnit(
						PressureUnit.Pascal);

				iteratedPressureDropValPascal =
					pressureDrop.As(PressureUnit.Pascal);

				double functionValue =
					iteratedPressureDropValPascal -
					dynamicPressureDropValuePascal;

				return functionValue;
			}

			double massFlowValueKgPerS;
			massFlowValueKgPerS = FindRoots.OfFunction(pressureDropRoot,
					-1e12,1e12);

			MassFlow massFlowrate;
			massFlowrate = new MassFlow(massFlowValueKgPerS,
					MassFlowUnit.KilogramPerSecond);
			// after i'm done, do some cleanup operations
			this.dynamicPressureDropValuePascal = 0.0;


			// and finally let me return the mass flowrate
				if (isNegativePressure)
				{
					return massFlowrate * -1.0;
				}
			return massFlowrate;
		}

		public double dynamicPressureDropValuePascal = 0.0;

    }
}
