using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharp.Components.Subcircuits;
using System.Linq;
using EngineeringUnits;
using EngineeringUnits.Units;
using MathNet.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A subcircuit that can contain a collection of entities.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
	///
	/// Note that this MockFluidSubCircuit class is just a copy of SubCircuit
	/// it doesn't change any code other than the class name,
	/// two overloads of constructors
	///
	/// And also the clone method
	/// which returns a new MockFluidSubCircuit rather than
	/// returning a SubCircuit
    public partial class FluidParallelSubCircuit : FluidEntity, 
	IParameterized<Parameters>,
	IComponent,
	IRuleSubject
    {
        private string[] _connections;

        /// <inheritdoc/>
        public Parameters Parameters { get; private set; } = new Parameters();

        /// <inheritdoc/>
        public string Model { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> Nodes => new ReadOnlyCollection<string>(_connections);

		// here is where i put my implementations for FluidEntity

		public override Pressure getPressureDrop(
				MassFlow flowrate){

			// first is that we get the definition object
			// now i have a fluid entity collection, i can
			// now obtain massflowrates by setting a pressure drop value
			//

			throw new NotImplementedException();
		}


		public override SpecificEnergy getKinematicPressureDrop(
				MassFlow flowrate){

			// this method guesses kinematic pressureDrop given a mass
			// flowrate
			// it is an implicit method
			//
			// 
			//
			this.massFlowrateValueForKinematicPressureDrop 
				= flowrate.As(MassFlowUnit.KilogramPerSecond);

			double massFlowRoot(double pressureDropValueJoulePerKg){
				
				// this is the value of the mass flowrate the 
				// iterated mass flowrate should match
				double massFlowValueKgPerS 
					= this.massFlowrateValueForKinematicPressureDrop;

				// this is the iterated mass flowrate value

				double iteratedMassFlowrateValueKgPerS;

				// and to get a value of iterated massflowrate value
				// I use pressure drop
				SpecificEnergy _pressureDrop;
				_pressureDrop = new SpecificEnergy(
						pressureDropValueJoulePerKg,
						SpecificEnergyUnit.JoulePerKilogram);

				MassFlow _massFlowRate;
				_massFlowRate = this.getMassFlowRate(_pressureDrop);

				// now i have the mass flowrate, i can change it to a double

				iteratedMassFlowrateValueKgPerS =
					_massFlowRate.As(MassFlowUnit.KilogramPerSecond);
				
				double functionValue =
					iteratedMassFlowrateValueKgPerS -
					massFlowValueKgPerS;

				return functionValue;

			}

			double pressureDropValueJoulePerKg;
			pressureDropValueJoulePerKg = FindRoots.OfFunction(
					massFlowRoot, -1e12, 1e12);
			
			SpecificEnergy _finalPressureDropvalue
				= new SpecificEnergy(pressureDropValueJoulePerKg,
						SpecificEnergyUnit.JoulePerKilogram);

			// after this i'll do some cleanup operations
			this.massFlowrateValueForKinematicPressureDrop = 0.0;


			return _finalPressureDropvalue;
		}

		public double massFlowrateValueForKinematicPressureDrop;

		public override MassFlow getMassFlowRate(SpecificEnergy kinematicPressureDrop){
			// how do i get a mass flowrate from a parallel circuit
			// (assuming all FluidEntities are in parallel?)
			//
			// I'll probbably need to get a FluidEntityCollection,
			// apply a pressure drop across all of them to get the
			// mass flowrate,
			// then sum them up altogether.
			// first is that we get the definition object
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);
			// i'm only reading the Entities, not writing to it
			// so should be thread safe unless i set it.
			// Now i want to check  all the fluidEntities within the
			// fluidEntityCollection

			List<MassFlow> massFlowList = new List<MassFlow>();

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the massflow object
				MassFlow _massFlowForOneEntity =
					_fluidEntity.getMassFlowRate(kinematicPressureDrop);

				// convert it to kg/s just to be sure

				_massFlowForOneEntity = _massFlowForOneEntity.ToUnit(
						MassFlowUnit.KilogramPerSecond);

				// lastly i add it to the massFlowList
				massFlowList.Add(_massFlowForOneEntity);

			}

			// once i'm done adding the massflow list i can sum up all the values
			MassFlow _totalMassFlowrate = new MassFlow(0.0, 
					MassFlowUnit.KilogramPerSecond);
			foreach (MassFlow _massFlowRate in massFlowList)
			{
				_totalMassFlowrate += _massFlowRate;
			}

			// then i want to check whether the fluid 


			return _totalMassFlowrate;
		}

		public override MassFlow getMassFlowRate(Pressure dynamicPressureDrop){
			// I'll probbably need to get a FluidEntityCollection,
			// apply a pressure drop across all of them to get the
			// mass flowrate,
			// then sum them up altogether.
			// first is that we get the definition object
			IFluidEntityCollection _fluidEntityCollection = 
				this.getFluidEntityCollection(this.Parameters.Definition.
						Entities);
			// i'm only reading the Entities, not writing to it
			// so should be thread safe unless i set it.
			// Now i want to check  all the fluidEntities within the
			// fluidEntityCollection

			List<MassFlow> massFlowList = new List<MassFlow>();

			foreach (var keyValuePair in _fluidEntityCollection._fluidEntities)
			{
				// the fluidEntities contain key value pairs of fluid entities
				// and strings. I extract the fluid entity first
				IFluidEntity _fluidEntity = 
					keyValuePair.Value;

				// i take the massflow object
				MassFlow _massFlowForOneEntity =
					_fluidEntity.getMassFlowRate(dynamicPressureDrop);

				// convert it to kg/s just to be sure

				_massFlowForOneEntity = _massFlowForOneEntity.ToUnit(
						MassFlowUnit.KilogramPerSecond);

				// lastly i add it to the massFlowList
				massFlowList.Add(_massFlowForOneEntity);

			}

			// once i'm done adding the massflow list i can sum up all the values
			MassFlow _totalMassFlowrate = new MassFlow(0.0, 
					MassFlowUnit.KilogramPerSecond);
			foreach (MassFlow _massFlowRate in massFlowList)
			{
				_totalMassFlowrate += _massFlowRate;
			}

			// then i want to check whether the fluid 


			return _totalMassFlowrate;
		}

		public IFluidEntityCollection getFluidEntityCollection(
				IEntityCollection _entityCollection){

			// i'll then like to check if these entities are okay 
			// to be in a fluidEntityList
			// but IFluidEntityCollection already has such a thing,
			// so i may as well use what's already there
			//

			IFluidEntityCollection _fluidEntityCollection;
			_fluidEntityCollection = new FluidEntityCollection();

			foreach (IEntity item in _entityCollection)
			{
				_fluidEntityCollection.Add(item);
			}

			return _fluidEntityCollection;
		}


        /// <summary>
        /// Gets the node map.
        /// </summary>
        /// <value>
        /// The node map.
        /// </value>
        /// <exception cref="NodeMismatchException">Thrown if the number of nodes don't match.</exception>
        private Bridge<string>[] NodeMap
        {
            get
            {
                if (Parameters.Definition == null)
                    return Array<Bridge<string>>.Empty();

                // Make a list of node bridges
                var pins = Parameters.Definition.Pins;
                var outNodes = _connections;
                if ((outNodes == null && pins.Count > 0) || outNodes.Length != pins.Count)
                    throw new NodeMismatchException(pins.Count, outNodes?.Length ?? 0);
                var nodes = new Bridge<string>[pins.Count];
                for (var i = 0; i < pins.Count; i++)
                    nodes[i] = new Bridge<string>(pins[i], outNodes[i]);
                return nodes;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="definition">The subcircuit definition.</param>
        /// <param name="nodes">The nodes that the subcircuit is connected to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="definition"/> is <c>null</c>.</exception>
        public FluidParallelSubCircuit(string name, ISubcircuitDefinition definition, params string[] nodes)
            : base(name)
        {
            Parameters.Definition = definition.ThrowIfNull(nameof(definition));
			this.constructInterpolateBeFromRe();
            Connect(nodes);
        }

        private FluidParallelSubCircuit(string name)
            : base(name)
        {
			this.constructInterpolateBeFromRe();
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            if (Parameters.Definition != null && Parameters.Definition.Entities.Count > 0)
            {
                // Create our local simulation and binding context to allow our behaviors to do stuff
                var localSim = new SubcircuitSimulation(Name, simulation, Parameters.Definition, NodeMap);
                var context = new SubcircuitBindingContext(this, localSim, behaviors);

                // Add the necessary behaviors
                behaviors.Add(new EntitiesBehavior(context));
                behaviors.Build(simulation, context)
                    .AddIfNo<ITemperatureBehavior>(context => new SpiceSharp.Components.Subcircuits.Temperature(context))
                    .AddIfNo<IAcceptBehavior>(context => new Accept(context))
                    .AddIfNo<ITimeBehavior>(context => new Time(context))
                    .AddIfNo<IBiasingBehavior>(context => new Biasing(context))
                    .AddIfNo<IBiasingUpdateBehavior>(context => new BiasingUpdate(context))
                    .AddIfNo<IFrequencyBehavior>(context => new SpiceSharp.Components.Subcircuits.Frequency(context))
                    .AddIfNo<IFrequencyUpdateBehavior>(context => new FrequencyUpdate(context))
                    .AddIfNo<INoiseBehavior>(context => new Subcircuits.Noise(context));

                // Run the simulation
                localSim.Run(Parameters.Definition.Entities);

                // Allow the behaviors to fetch the behaviors if they want
                foreach (var behavior in behaviors)
                {
                    if (behavior is ISubcircuitBehavior subcktBehavior)
                        subcktBehavior.FetchBehaviors(context);
                }
            }
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <inheritdoc/>
        public IComponent Connect(params string[] nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            _connections = new string[nodes.Length];
            for (var i = 0; i < nodes.Length; i++)
                _connections[i] = nodes[i].ThrowIfNull($"node {0}".FormatString(i + 1));
            return this;
        }

        /// <inheritdoc/>
        public void Apply(IRules rules)
        {
            if (Parameters.Definition == null)
                return;

            var crp = rules.GetParameterSet<ComponentRuleParameters>();
            var newRules = new SubcircuitRules(rules, new ComponentRuleParameters(
                new VariableFactory(Name, crp.Factory, NodeMap, crp.Comparer),
                crp.Comparer));
            foreach (var c in Parameters.Definition.Entities)
            {
                if (c is IRuleSubject subject)
                    subject.Apply(newRules);
            }
        }

        /// <inheritdoc/>
        public override IEntity Clone()
        {
            return new FluidParallelSubCircuit(Name)
            {
                Parameters = Parameters.Clone(),
                _connections = (string[])_connections.Clone(),
                Model = Model
            };
        }
    }
}
