using SpiceSharp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using EngineeringUnits;
using EngineeringUnits.Units;
using MathNet.Numerics;

namespace SpiceSharp
{
    /// <summary>
    /// Represents an electronic circuit.
    /// </summary>
    /// <seealso cref="IEntityCollection" />
    public class FluidSeriesCircuit : 
		IEntityCollection,
		IFluidEntityCollection
    {
        private readonly IFluidEntityCollection _entities;

		public Dictionary<string, IFluidEntity> _fluidEntities 
		{
			// basically i'm forcing the user to interact
			// with the dictionary within the _entities 
			// FluidEntityCollection
			get {
				return _entities._fluidEntities;
			}

			set {
				_entities._fluidEntities = value;
			}
		}
		// this is where the fluidEntityCollection is meant to
		// return pressure drops (kinematic or otherwise)
		//

		public Pressure getPressureDrop(
				MassFlow massFlowrate){
			Pressure totalPressureDrop;
			totalPressureDrop = new Pressure(0.0, 
					PressureUnit.Pascal);
			foreach (var fluidEntityDictEntry in _fluidEntities)
			{
				totalPressureDrop += fluidEntityDictEntry.Value.getPressureDrop(
						massFlowrate);
			}

			return totalPressureDrop;
		}

		public SpecificEnergy getKinematicPressureDrop(
				MassFlow massFlowrate){
			SpecificEnergy totalPressureDrop;
			totalPressureDrop = new SpecificEnergy(0.0, 
					SpecificEnergyUnit.JoulePerKilogram);
			foreach (var fluidEntityDictEntry in _fluidEntities)
			{
				totalPressureDrop += fluidEntityDictEntry.
					Value.getKinematicPressureDrop(
						massFlowrate);
			}

			return totalPressureDrop;
		}

		// here is where i can get mass flowrate values from pressure drops
		//

		MassFlow IFluidEntityCollection.getMassFlowRate(
				Pressure dynamicPressureDrop){


			// At first glance the solution to this algorithm is simple:
			// convert the dynamic pressure drop to a kinematic
			// pressure drop using the density of each fluid entity
			// However that means i need  to put a get Presusre Code in each
			// fluid entity
			// it's probably better to just adopt the code used in 
			// getMassFlowrate(kinematicPressureDrop)
			// to do the same
			//

			// first let me store the dynamicPressureDropValue as 
			// a double


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
			return massFlowrate;
		}

		public double dynamicPressureDropValuePascal = 0.0;






		MassFlow IFluidEntityCollection.getMassFlowRate(
				SpecificEnergy kinematicPressureDrop){
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
			return massFlowrate;

		}

		public double kinematicPressureDropValJoulePerKg = 0.0;

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => _entities.IsReadOnly;

        /// <inheritdoc/>
        public IEntity this[string name] => _entities[name];

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        public FluidSeriesCircuit()
        {
            _entities = new FluidEntityCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        public FluidSeriesCircuit(IFluidEntityCollection entities)
        {
            _entities = entities.ThrowIfNull(nameof(entities));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public FluidSeriesCircuit(IEnumerable<IEntity> entities)
            : this()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public FluidSeriesCircuit(params IEntity[] entities)
            : this()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Merge a circuit with this one. Entities are merged by reference!
        /// </summary>
        /// <param name="ckt">The circuit to merge with.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ckt"/> is <c>null</c>.</exception>
        public void Merge(Circuit ckt)
        {
            ckt.ThrowIfNull(nameof(ckt));
            foreach (var entity in ckt)
                Add(entity);
        }

        /// <inheritdoc/>
        public bool Remove(string name) => _entities.Remove(name);

        /// <inheritdoc/>
        public bool Contains(string name) => _entities.Contains(name);

        /// <inheritdoc/>
        public bool TryGetEntity(string name, out IEntity entity) => _entities.TryGetEntity(name, out entity);

        /// <inheritdoc/>
        public IEnumerable<E> ByType<E>() where E : IEntity => _entities.ByType<E>();

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        public void Add(IEntity item) => _entities.Add(item);

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _entities.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.
        /// </returns>
        public bool Contains(IEntity item) => _entities.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(IEntity[] array, int arrayIndex) => _entities.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        public bool Remove(IEntity item) => _entities.Remove(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IEntity> GetEnumerator() => _entities.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entities).GetEnumerator();

        /// <inheritdoc/>
        public IEntityCollection Clone()
            => new Circuit(_entities.Clone());
    }
}
