using System;
using System.Collections.Generic;
using System.Numerics;

namespace Kobanan
{
    public struct FilterMask : IEqualityComparer<FilterMask>
    {
            
        private BigInteger _includeMask;
        private BigInteger _excludeMask;
        private int _hash;

        public bool Equals(FilterMask x, FilterMask y)
        {
            return x._includeMask.Equals(y._includeMask) && x._excludeMask.Equals(y._excludeMask);
        }

        public int GetHashCode(FilterMask obj) => _hash;

        private void UpdateHash()
        {
            _hash = _includeMask.GetHashCode() + _excludeMask.GetHashCode();
        }

        public void IncludeComponent(ComponentId includeComponent)
        {
            IncludeMask(includeComponent.MaskId);
        }
        
        public void IncludeMask(BigInteger mask)
        {
            _includeMask |= mask;
        }

        public void ExcludeComponent(ComponentId excludeComponent)
        {
            _excludeMask |= excludeComponent.MaskId;
        }

        public void End()
        {
            UpdateHash();
        }

        // _includeMask        00101    ~ 11010  
        // _exclude            10000    ~ 01111
        // entityComponentMask 10100      10100 
        // &                   00100      11110    
        // |                   10101 
        
        
        //_exclude             00001
        // entityComponentMask 10100
        public bool Contains(BigInteger entityComponentMask)
        {
            var allInclude = (~(~_includeMask | entityComponentMask)).IsZero;
            var anyExclude = !(_excludeMask & entityComponentMask).IsZero;
            return allInclude && !anyExclude;
        }
    }

    public struct FilterBuilder : IFilterBuilder
    {
        private IWorld _world;

        private FilterMask _filterMask;

        private Dictionary<FilterMask, IFilter> _filters;

        public FilterBuilder(IWorld world)
        {
            _filters = new Dictionary<FilterMask, IFilter>(new FilterMask());
            _world = world;
            _filterMask = default;
        }
        public IFilter End()
        {
            _filterMask.End();
            if (!_filters.TryGetValue(_filterMask, out var filter))
            {
                _filters.Add(_filterMask, filter = new Filter(_filterMask));
                _world.OnFilterCreated(filter);
            }
            return filter;
        }

        public IFilterBuilder Inc<T>()
        {
            var id = IdProvider.GetIdByType<T>();
            _filterMask.IncludeComponent(id);
            return this;
        }
        
        public IFilterBuilder Inc(BigInteger includeMask)
        {
            _filterMask.IncludeMask(includeMask);
            return this;
        }

        public IFilterBuilder Exc<T>()
        {
            var id = IdProvider.GetIdByType<T>();
            _filterMask.ExcludeComponent(id);
            return this;
        }

        public IFilterBuilder Des<T>()
        {
            return this;
        }

        public IFilterBuilder Child<T>()
        {
            return this;
        }

        
    }
}