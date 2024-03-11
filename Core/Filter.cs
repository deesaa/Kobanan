using System;
using System.Collections;
using System.Collections.Generic;

namespace Kobanan
{
    
    public struct FilterEnumerator : IEnumerator<IEntity>
    {
        private IEntity[] _entities;
        private int _currentIndex;
        private int _size;
        
        public FilterEnumerator(IEntity[] entities, int size)
        {
            _entities = entities;
            _size = size;
            Current = null;
            _currentIndex = -1;
        }
        
        public void Dispose()
        {
            _entities = null;
            Current = null;
            _currentIndex = -1;
        }

        public bool MoveNext()
        {
            
            _currentIndex++;
            if (_currentIndex >= _size || _currentIndex >= _entities.Length) return false;
            Current = _entities[_currentIndex];
            return true;
            
        }

        public void Reset()
        {
            Current = null;
            _currentIndex = -1;
        }

        public IEntity Current { get; private set; }

        object IEnumerator.Current => Current;
    }

    public class Filter : IFilter
    {
        private const int DenseArrayStepSize = 32;
        
        private FilterMask _filterMask;

        private Dictionary<Euid, int> _entitiesListMap = new();
        //private List<IEntity> _sparseEntities = new();
        private IEntity[] _denseEntities = new IEntity[DenseArrayStepSize];
        private int _entitiesCount = 0;
        
        
       // private int[] _recycleDenseIndexes = new int[8];
        //private int _recycleDenseIndexesSize = 0;
        
        public Filter(FilterMask filterMask, string filterName)
        {
            _filterMask = filterMask;
            FilterName = filterName;
            FilterId = new Fuid(filterMask.GetHashCode());
        }

        public FilterEnumerator GetEnumerator() => new FilterEnumerator(_denseEntities, _entitiesCount); // Need custom
        public void AddEntity(IEntity entity)
        {
            var entityInFilterId = _entitiesCount;
            var euid = entity.Euid;
            
            if (entityInFilterId >= _denseEntities.Length)
            {
                var k = entityInFilterId / DenseArrayStepSize;
                var newSize = DenseArrayStepSize * (k + 1);
                Array.Resize(ref _denseEntities, newSize);
            }
            
            _denseEntities[entityInFilterId] = entity;
            _entitiesListMap[euid] = entityInFilterId;
            _entitiesCount++;
            entity.OnAddInFilter(this);
        }
        
        public void RemoveEntity(IEntity entity)
        {
            var euid = entity.Euid;
            var entityInFilterId = _entitiesListMap[euid];
            _denseEntities[entityInFilterId] = null;
            _entitiesListMap.Remove(euid);
            entity.OnRemoveFromFilter(this);
        }

        public string FilterName { get; }
        public Fuid FilterId { get; }
        public FilterMask GetMask() => _filterMask;
    }
}