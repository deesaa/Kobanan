using System;
using System.Collections.Generic;

namespace Kobanan
{
    public struct Fuid 
    {
        public readonly int Id;

        public Fuid(int id)
        {
            Id = id;
        }
    }
    
    public interface IFilter
    {
        string FilterName { get; }
        Fuid FilterId { get; }
        FilterMask GetMask();
        FilterEnumerator GetEnumerator();
        void AddEntity(IEntity entity);
        void RemoveEntity(IEntity entity);
    }

    public interface IFiltersPool
    {
        IFilter GetFilter(FilterMask filterMask);
        void OnComponentAdded(IEntity entity, IComponentBase component, ComponentId componentId);
        void OnComponentDeleted(IEntity entity, IComponentBase component, ComponentId componentId);
    }
    
    public class FiltersPool : IFiltersPool
    {
        private Dictionary<FilterMask, IFilter> _filters = new();
        private IWorld _world;

        public FiltersPool(IWorld world)
        {
            _world = world;
        }

        public IFilter GetFilter(FilterMask filterMask)
        {
            if (!_filters.TryGetValue(filterMask, out var filter))
            {
                _filters.Add(filterMask, filter = new Filter(filterMask, "SomeFilter"));
                _world.OnFilterCreated(filter);
            }
            return filter;
        }

        public void OnComponentAdded(IEntity entity, IComponentBase component, ComponentId componentId)
        {
            var entityComponentMask = entity.GetComponentsEntityMask();
            foreach (var filterPair in _filters)
            {
                var filter = filterPair.Value;
                var filterMask = filter.GetMask();
                if(filterMask.Contains(entityComponentMask))
                    filter.AddEntity(entity);
            }
        }

        public void OnComponentDeleted(IEntity entity, IComponentBase component, ComponentId componentId)
        {
            foreach (var filter in entity.Filters())
            {
                var filterMask = filter.GetMask();
                var entityComponentMask = entity.GetComponentsEntityMask();
                if(filterMask.Contains(entityComponentMask))
                    filter.RemoveEntity(entity);
            }
        }
    }
}