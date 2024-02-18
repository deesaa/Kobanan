using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Kobanan
{
    
    public struct FilterEnumerator : IEnumerator<IEntity>
    {
        private List<IEntity> _entities;
        private int _currentIndex;
        
        public FilterEnumerator(List<IEntity> entities)
        {
            _entities = entities;
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
            if (_currentIndex <= _entities.Count) return false;
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
        private FilterMask _filterMask;

        private List<IEntity> _entities = new();
        
        public Filter(FilterMask filterMask)
        {
            _filterMask = filterMask;
        }

        public FilterEnumerator GetEnumerator() => new FilterEnumerator(_entities);

        public FilterMask GetMask() => _filterMask;
    }
}