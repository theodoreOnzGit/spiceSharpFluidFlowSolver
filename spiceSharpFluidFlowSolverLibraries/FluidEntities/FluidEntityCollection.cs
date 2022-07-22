using System;
using System.Collections;
using System.Collections.Generic;
using EngineeringUnits;
using EngineeringUnits.Units;

// to help the FluidEntityCollection implement both 
// IEntityCollection and ICollection<IFluidEntity>
// I have two separate lists or dictionaries
// such that if i add a fluid entity,
//
// i add the fluidEntity to both the _entity
// and _fluidEntity dictionaries
//
// And if i remove a fluid entity, then i remove
// it from both lists as well
// But if i add and remove normal IEntities from them
// they will not affect the fluidEntity Dictionary
//
// The fluidEntity Dictionary is only here to ensure
// that when i call the getPressureDrop methods
// i'm able to sum everything up


namespace SpiceSharp.Entities
{
    /// <summary>
    /// A default implementation for <see cref="IEntityCollection"/>.
    /// </summary>
    /// <seealso cref="IEntityCollection" />
    public class FluidEntityCollection : IEntityCollection,
	IFluidEntityCollection
    {
        private readonly Dictionary<string, IEntity> _entities;
		public Dictionary<string, IFluidEntity> _fluidEntities { get; set; }


		// this is where the fluidEntityCollection is meant to
		// return pressure drops (kinematic or otherwise)
		//

		Pressure IFluidEntityCollection.getPressureDrop(
				MassFlow massFlowrate){
			Pressure totalPressureDrop;
			totalPressureDrop = new Pressure(0.0, 
					PressureUnit.SI);
			foreach (var fluidEntityDictEntry in _fluidEntities)
			{
				totalPressureDrop += fluidEntityDictEntry.Value.getPressureDrop(
						massFlowrate);
			}

			return totalPressureDrop;
		}

		SpecificEnergy IFluidEntityCollection.getKinematicPressureDrop(
				MassFlow massFlowrate){
			SpecificEnergy totalPressureDrop;
			totalPressureDrop = new SpecificEnergy(0.0, 
					SpecificEnergyUnit.SI);
			foreach (var fluidEntityDictEntry in _fluidEntities)
			{
				totalPressureDrop += fluidEntityDictEntry.Value.getPressureDrop(
						massFlowrate);
			}

			return totalPressureDrop;
		}

        /// <summary>
        /// Occurs when an entity has been added.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityAdded;

        /// <summary>
        /// Occurs when an entity has been removed.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityRemoved;

        /// <inheritdoc/>
        public IEntity this[string name] => _entities[name];

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<string> Keys => _entities.Keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        public FluidEntityCollection()
        {
            _entities = new Dictionary<string, IEntity>(StringComparer.OrdinalIgnoreCase);
            _fluidEntities = new Dictionary<string, IFluidEntity>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public FluidEntityCollection(IEqualityComparer<string> comparer)
        {
            _entities = new Dictionary<string, IEntity>(comparer ?? Constants.DefaultComparer);
            _fluidEntities = new Dictionary<string, IFluidEntity>(comparer ?? Constants.DefaultComparer);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _entities.Clear();

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if another entity with the same name already exists.</exception>

        public void Add(IEntity item)
        {
			// Also if i add an IEntity object which happens to
			// implement the IFluidEntity interface, 
			// i also want to add it to the dictionary.


            item.ThrowIfNull(nameof(item));
            try
            {
                _entities.Add(item.Name, item);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(Properties.Resources.EntityCollection_KeyExists.FormatString(item.Name));
            }
            OnEntityAdded(new EntityEventArgs(item));

			this.TryAddToFluidEntityDict(item);

        }

		// this function helps me check if an IEntity is a IFluidEntity
		// if it is, then add it to the Dictionary
		// if not, then do nothing

		public void TryAddToFluidEntityDict(IEntity item){

			// i want to see if the item
			// is castable as an IFluidEntity
			// I will declare the type first

			IFluidEntity fluidEntity;

			bool isFluidEntity = this.TryCastToIFluidEntity(
					item, 
					out fluidEntity);

			if (isFluidEntity)
			{
				_fluidEntities.Add(fluidEntity.Name, fluidEntity);
				return;
			}

			return;

		}

		// This function is here to try and cast the entity
		// type to IFluidEntity.
		//
		public bool TryCastToIFluidEntity(IEntity item, 
			out IFluidEntity result){

			if (item is IFluidEntity)
			{
				result = (IFluidEntity)item;
				return true;
			}
			result = default(IFluidEntity);
			return false;

		}

		// Now for the remove bits, i can remove an entry
		// by name or by object
		//
		// If removing by name, i want to check both lists
		// if the name exists, and remove the entry from
		// both lists

        /// <inheritdoc/>
        public bool Remove(string name)
        {
            name.ThrowIfNull(nameof(name));
            if (!_entities.TryGetValue(name, out var entity))
                return false;
            _entities.Remove(name);
            OnEntityRemoved(new EntityEventArgs(entity));
			
			// once i removed the entities, great,
			// i want to check if the name is present in
			// the _fluidEntities dictionary
			// if it's there, remove it
			// else, do nothing
			//
            if (_fluidEntities.TryGetValue(name, out var 
						fluidEntity))
				_fluidEntities.Remove(name);

            return true;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            if (!_entities.TryGetValue(item.Name, out var result) || result != item)
                return false;
            _entities.Remove(item.Name);
            OnEntityRemoved(new EntityEventArgs(item));

			// now to this method, i also want to remove
			// the fluidEntity if it is castable as a fluidEntity
			// else do nothing
			this.TryRemoveFluidEntityDict(item);

            return true;
        }


		// this function removes an entity from the fluidEntity
		// Dictionary if 
		public void TryRemoveFluidEntityDict(IEntity item){

			// i want to see if the item
			// is castable as an IFluidEntity
			// I will declare the type first

            item.ThrowIfNull(nameof(item));
			IFluidEntity fluidEntity;

			bool isFluidEntity = this.TryCastToIFluidEntity(
					item, 
					out fluidEntity);

			// if it's a fluidEntity, then try to remove it
			// from the dictionary,
			// we also check if the fluidEntity name is found
			// inside the dictionary
			// if it's not found, don't do anything
			//
            if (!_fluidEntities.TryGetValue(fluidEntity.Name, 
						out var result) || result != item)
                return;
			if (isFluidEntity)
			{
				_fluidEntities.Remove(fluidEntity.Name);
				return;
			}

			return;

		}



        /// <inheritdoc/>
        public bool Contains(string name) => _entities.ContainsKey(name);

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
        public bool Contains(IEntity entity)
        {
            entity.ThrowIfNull(nameof(entity));
            if (_entities.TryGetValue(entity.Name, out var result))
                return result == entity;
            return false;
        }
		// this overload isn't really used much
		// but can check whether the fluid entity list contains
		// a fluid entity object
        public bool Contains(IFluidEntity fluidEntity)
        {
            fluidEntity.ThrowIfNull(nameof(fluidEntity));
            if (_fluidEntities.TryGetValue(fluidEntity.Name, out var result))
                return result == fluidEntity;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetEntity(string name, out IEntity entity) => _entities.TryGetValue(name, out entity);

        /// <inheritdoc/>
        public IEnumerable<E> ByType<E>() where E : IEntity
        {
            foreach (var entity in _entities.Values)
            {
                if (entity is E e)
                    yield return e;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<IEntity> GetEnumerator() => _entities.Values.GetEnumerator();


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The array index.</param>
        void ICollection<IEntity>.CopyTo(IEntity[] array, int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < arrayIndex + Count)
                throw new ArgumentException(Properties.Resources.NotEnoughElements);
            foreach (var item in _entities.Values)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Raises the <see cref="EntityAdded" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EntityEventArgs"/> instance containing the event data.</param>
        protected virtual void OnEntityAdded(EntityEventArgs args) => EntityAdded?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="EntityRemoved" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EntityEventArgs"/> instance containing the event data.</param>
        protected virtual void OnEntityRemoved(EntityEventArgs args) => EntityRemoved?.Invoke(this, args);

        /// <inheritdoc/>
        public IEntityCollection Clone()
        {
            var clone = new EntityCollection(_entities.Comparer);
            foreach (var entity in _entities.Values)
                clone.Add(entity);
            return clone;
        }
    }
}
