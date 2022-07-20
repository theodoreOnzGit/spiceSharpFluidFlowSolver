using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components.FM40Behaviors
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
		private IfLDKFactorJacobian _jacobianObject;

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
			_jacobianObject = new flowmeterFM40Jacobian();

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

		public double relaxationFactor = 0.3;

        void IBiasingBehavior.Load()
        {
            // First get the current iteration voltage, but for basepipe
			// , this is actually pressuredrop
            var deltaP = _nodeA.Value - _nodeB.Value;
			// let's include height also
			// gz is the hydrostatic kinematic pressure increment from
			// node A to node B
			Length componentLength = _bp.componentLength;
			double gz;
			// of course g is 9.81 m/s^2
			// we note that z = L sin \theta
			gz = 9.81 * componentLength.As(LengthUnit.SI) *
				Math.Sin(_bp.inclineAngle.As(AngleUnit.Radian));

			deltaP -= gz;

			SpecificEnergy pressureDrop;
			pressureDrop = new SpecificEnergy(deltaP, SpecificEnergyUnit.SI);

            // Calculate the derivative w.r.t. one of the voltages
            var isNegative = deltaP < 0;
			// c here is current, but 
			// we don't really use it
			// the equivalent is to calculate mass flowrate
			// so first let's calculate a Bejan number
			//

			Length hydraulicDiameter;
			hydraulicDiameter = _bp.hydraulicDiameter;

			KinematicViscosity fluidKinViscosity;
			fluidKinViscosity = _bp.fluidKinViscosity;

			double bejanNumber;
			bejanNumber = _jacobianObject.getBejanNumber(
					pressureDrop,
					fluidKinViscosity,
					hydraulicDiameter);

			double Re = _jacobianObject.getRe(bejanNumber);


			Area crossSectionalArea;
			crossSectionalArea = _bp.crossSectionalArea();

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




			double dm_dPA = _jacobianObject.dm_dPA(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					fluidKinViscosity);

			double dm_dPB = _jacobianObject.dm_dPB(crossSectionalArea,
					fluidViscosity,
					hydraulicDiameter,
					pressureDrop,
					fluidKinViscosity);

			double minus_dm_dPA = -dm_dPA;

			double minus_dm_dPB = -dm_dPB;

            // In order to avoid having a singular matrix, we want to have at least a very small value here.
            //g = Math.Max(g, _baseConfig.Gmin);
            dm_dPA = Math.Max(dm_dPA, _baseConfig.Gmin);
            dm_dPB = Math.Max(dm_dPB, _baseConfig.Gmin);
            minus_dm_dPA = Math.Max(minus_dm_dPA, _baseConfig.Gmin);
            minus_dm_dPB = Math.Max(minus_dm_dPB, _baseConfig.Gmin);

			//  if pressure difference is reversed
			// mass flowrate is also reversed
			//if (isNegative)
			//	massFlowRateValue = -massFlowRateValue;
			// this was originally to ensure current or massflowrate is positive.
			// but it prevents us from seeing if backflow is possible

            // Load the RHS vector

			// so for the mass flowrate case we use:
			// RHS term 1 is the
			// mass flowrate out of node A
			// minus the jacobian times kinematic pressure 
			// difference
			// at the resistor
			double nodeARHSTerm;
			nodeARHSTerm = -massFlowRateValue + dm_dPA * _nodeA.Value +
				dm_dPB * (_nodeB.Value - gz);

			double nodeBRHSTerm;
			nodeBRHSTerm = massFlowRateValue + minus_dm_dPA * _nodeA.Value +
				minus_dm_dPB * (_nodeB.Value - gz);


            this._elements.Add(
                // Y-matrix
                dm_dPA, dm_dPB, minus_dm_dPA, minus_dm_dPB,
                // RHS-vector
                nodeARHSTerm, nodeBRHSTerm);
        }
    }
}
