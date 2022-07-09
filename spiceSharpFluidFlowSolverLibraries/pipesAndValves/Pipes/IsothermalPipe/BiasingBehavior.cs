using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components.IsothermalPipeBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="NonlinearResistor"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly IVariable<double> _nodeA, _nodeB;
        private readonly ElementSet<double> _elements;
        private readonly BaseParameters _bp;
        private readonly BiasingParameters _baseConfig;
		private IFrictionFactorJacobian _jacobianObject;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Get our resistor parameters (we want our A and B parameter values)
            _bp = context.GetParameterSet<BaseParameters>();

            // Get the simulation parameters (we want to use Gmin)
            _baseConfig = context.GetSimulationParameterSet<BiasingParameters>();

			// Construct the IFrictionFactorJacobian object
			_jacobianObject = new ChurchillFrictionFactorJacobian();

            // Request the node variables
            var state = context.GetState<IBiasingSimulationState>();
            _nodeA = state.GetSharedVariable(context.Nodes[0]);
            _nodeB = state.GetSharedVariable(context.Nodes[1]);

            // We need 4 matrix elements and 2 RHS vector elements
            var indexA = state.Map[_nodeA];
            var indexB = state.Map[_nodeB];
            this._elements = new ElementSet<double>(state.Solver, new[] {
                    new MatrixLocation(indexA, indexA),
                    new MatrixLocation(indexA, indexB),
                    new MatrixLocation(indexB, indexA),
                    new MatrixLocation(indexB, indexB)
                }, new[] { indexA, indexB });
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // First get the current iteration voltage, but for basepipe
			// , this is actually pressuredrop
            var deltaP = _nodeA.Value - _nodeB.Value;
			SpecificEnergy pressureDrop;
			pressureDrop = new SpecificEnergy(deltaP, SpecificEnergyUnit.SI);

            // Calculate the derivative w.r.t. one of the voltages
            var isNegative = deltaP < 0;
			// c here is current, but 
			// we don't really use it
			// the equivalent is to calculate mass flowrate
			// so first let's calculate a Bejan number
			//

			Length pipeLength;
			pipeLength = _bp.pipeLength;

			KinematicViscosity fluidKinViscosity;
			fluidKinViscosity = _bp.fluidKinViscosity;

			double bejanNumber;
			// now here's an issue found during debugging,
			// if bejan number is zero, the simulation will crash
			// this is because the derivatives at Re = 0 are not
			// defined for the churchill correlation.
			// i will arbitrarily add 1000 to the bejan number
			// if it is zero so that we can continue simulation
			if(pressureDrop.As(SpecificEnergyUnit.SI) == 0){
				pressureDrop = new SpecificEnergy(100
						,SpecificEnergyUnit.SI);
			}
			bejanNumber = _jacobianObject.getBejanNumber(
					pressureDrop,
					fluidKinViscosity,
					pipeLength);


			double roughnessRatio;
			roughnessRatio = _bp.roughnessRatio();

			double lengthToDiameter;
			lengthToDiameter = _bp.lengthToDiameter();

			void checkNumbers(double bejanNumber,
					double roughnessRatio,
					double lengthToDiameter){

				if(Double.IsNaN(bejanNumber))
					throw new Exception("bejanNumber is NaN");

				if(Double.IsNaN(roughnessRatio))
					throw new Exception("roughnessRatio is NaN");

				if(Double.IsNaN(lengthToDiameter))
					throw new Exception("lengthToDiameter is NaN");

				if(1 == 1){
					string errorMsg ="";
					errorMsg += "\n bejanNumber is " + bejanNumber.ToString();
					errorMsg += "\n roughnessRatio is " + roughnessRatio.ToString();
					errorMsg += "\n lengthToDiameter is " + lengthToDiameter.ToString();
					throw new Exception(errorMsg);
				}
			}

			// checkNumbers(bejanNumber, roughnessRatio, lengthToDiameter);

			// if Bejan number is zero, then Re is 0
			// However, for steady state, we don't want to have that 
			// equals to zero,
			// if not the simulation will crash
			// we'll just get

			double Re = 0;
			if(bejanNumber > 0){
				Re = _jacobianObject.getRe(bejanNumber, 
						roughnessRatio, 
						lengthToDiameter);
			}


			Area crossSectionalArea;
			crossSectionalArea = _bp.crossSectionalArea();

			Length hydraulicDiameter;
			hydraulicDiameter = _bp.hydraulicDiameter;

			DynamicViscosity fluidViscosity;
			fluidViscosity = _bp.fluidViscosity;

			MassFlow massFlowRate;
			// i noticed that dmdRe is the same
			// as mass/Re due to its linear relationship
			massFlowRate = _jacobianObject.dmdRe(
					crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter);
			massFlowRate *= Re;

			double massFlowRateValue;
			massFlowRateValue = massFlowRate.As(MassFlowUnit.SI);


			// For basepipe, we just calculate the jacobian straightaway
			// so we first load everything else from the base parameters


			Length absoluteRoughness;
			absoluteRoughness = _bp.absoluteRoughness;


			double dm_dPA = _jacobianObject.dm_dPA(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					absoluteRoughness,
					pipeLength,
					fluidKinViscosity);

			double dm_dPB = _jacobianObject.dm_dPB(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					absoluteRoughness,
					pipeLength,
					fluidKinViscosity);

			double minus_dm_dPA = -_jacobianObject.dm_dPA(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					absoluteRoughness,
					pipeLength,
					fluidKinViscosity);

			double minus_dm_dPB = -_jacobianObject.dm_dPB(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					absoluteRoughness,
					pipeLength,
					fluidKinViscosity);

            // In order to avoid having a singular matrix, we want to have at least a very small value here.
            //g = Math.Max(g, _baseConfig.Gmin);
            dm_dPA = Math.Max(dm_dPA, _baseConfig.Gmin);
            dm_dPB = Math.Max(dm_dPB, _baseConfig.Gmin);
            minus_dm_dPA = Math.Max(minus_dm_dPA, _baseConfig.Gmin);
            minus_dm_dPB = Math.Max(minus_dm_dPB, _baseConfig.Gmin);

			//  if pressure difference is reversed
			// mass flowrate is also reversed
			if (isNegative)
				massFlowRateValue = -massFlowRateValue;

            // Load the RHS vector

			// so for the mass flowrate case we use:
			// RHS term 1 is the
			// mass flowrate out of node A
			// minus the jacobian times kinematic pressure 
			// difference
			// at the resistor
			double nodeARHSTerm;
			nodeARHSTerm = -massFlowRateValue + dm_dPA * _nodeA.Value +
				dm_dPB * _nodeB.Value;

			double nodeBRHSTerm;
			nodeBRHSTerm = massFlowRateValue + minus_dm_dPA * _nodeA.Value +
				minus_dm_dPB * _nodeB.Value;


            this._elements.Add(
                // Y-matrix
                dm_dPA, dm_dPB, minus_dm_dPA, minus_dm_dPB,
                // RHS-vector
                nodeARHSTerm, nodeBRHSTerm);
        }
    }
}
