using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using EngineeringUnits;
using EngineeringUnits.Units;

namespace SpiceSharp.Components.TherminolPipeBehaviors
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
		private IFrictionFactorJacobian _jacobianObject(){
			return this._bp.jacobianObject();
		}

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
			throw new NotImplementedException();
        }
    }
}
