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
        private IFiltersPool _filtersPool;
        
        public FilterBuilder(IWorld world, IFiltersPool filtersPool)
        {
            _filtersPool = filtersPool;
            _world = world;
            _filterMask = default;
        }
        public IFilter End()
        {
            _filterMask.End();
            return _filtersPool.GetFilter(_filterMask);
        }

        public IFilterBuilder Inc<T>()
        {
            var id = IdProvider.GetIdByType<T>();
            _filterMask.IncludeComponent(id);
            return this;
        }
        
        public IFilterBuilder Inc(BigInteger includeComponentMask)
        {
            _filterMask.IncludeMask(includeComponentMask);
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