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
	IEnumerable<IFluidEntity>,
	ICollection<IFluidEntity>,
	IFluidEntityCollection
    {
        private readonly Dictionary<string, IEntity> _entities;
		private readonly Dictionary<string, IFluidEntity> _fluidEntities;


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
        public void Add(IFluidEntity item)
        {
			// first i make a copy of the item and cast it
			// into IEntity type

			IEntity entityItem;
			entityItem = (IEntity)item;

			this.Add(entityItem);

			_fluidEntities.Add(item.Name, item);
        }

        public void Add(IEntity item)
        {
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
        }

        /// <inheritdoc/>
        public bool Remove(string name)
        {
            name.ThrowIfNull(nameof(name));
            if (!_entities.TryGetValue(name, out var entity))
                return false;
            _entities.Remove(name);
            OnEntityRemoved(new EntityEventArgs(entity));
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
            return true;
        }

        public bool Remove(IFluidEntity item)
        {
			// first i make a copy of the item and cast it
			// into IEntity type

			IEntity entityItem;
			entityItem = (IEntity)item;

			this.Remove(entityItem);

            if (!_fluidEntities.TryGetValue(item.Name, out var result) || result != item)
                return false;
            _fluidEntities.Remove(item.Name);
            return true;
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
        public virtual IEnumerator<IEntity> GetEntityEnumerator() => _entities.Values.GetEnumerator();
        public virtual IEnumerator<IFluidEntity> GetFluidEntityEnumerator() => _fluidEntities.Values.GetEnumerator();

		IEnumerator<IEntity> IEnumerable<IEntity>.GetEnumerator() => GetEntityEnumerator();
		IEnumerator<IFluidEntity> IEnumerable<IFluidEntity>.GetEnumerator() => GetFluidEntityEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetFluidEntityEnumerator();
		

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

		// i don't see much use here, but i'll just implement the method
		// this will copy the fluid Entities to the exisitng fluidEntity
		// array, no entities from the entityList will be copied
        void ICollection<IFluidEntity>.CopyTo(IFluidEntity[] array, 
				int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < arrayIndex + Count)
                throw new ArgumentException(Properties.Resources.NotEnoughElements);
            foreach (var item in _fluidEntities.Values)
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
