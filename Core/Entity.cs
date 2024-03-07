using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Unity.VisualScripting;

namespace Kobanan
{
    public class ComponentsDictionary : IDictionary<ComponentId, IComponentBase>
    {
        private const int SparseArraySizeStep = 32;
        private IComponentBase[] _sparseComponents = new IComponentBase[SparseArraySizeStep];
        private List<IComponentBase> _denseComponents = new(8);
        public IEnumerator<KeyValuePair<ComponentId, IComponentBase>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<ComponentId, IComponentBase> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<ComponentId, IComponentBase> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<ComponentId, IComponentBase>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<ComponentId, IComponentBase> item)
        {
            throw new NotImplementedException();
        }

        public int Count => _denseComponents.Count;
        public bool IsReadOnly => false;
        public void Add(ComponentId key, IComponentBase value)
        {
            if (value == null)
            {
                Remove(key);
                return;
            }
            
            _denseComponents.Add(value);
            if (key.IncrementalId >= _sparseComponents.Length)
            {
                var k = key.IncrementalId / SparseArraySizeStep;
                var newSize = SparseArraySizeStep * (k + 1);
                Array.Resize(ref _sparseComponents, newSize);
            }
            _sparseComponents[key.IncrementalId] = value;
        }

        public bool ContainsKey(ComponentId key)
        {
            return _sparseComponents.Length > key.IncrementalId && _sparseComponents[key.IncrementalId] != null;
        }

        public bool Remove(ComponentId componentId)
        {
            if (_sparseComponents.Length <= componentId.IncrementalId) return false;
            _denseComponents.RemoveAll(component =>
            {
                var type = IdProvider.GetTypeById(componentId);
                return component.GetType() == type;
            });
            _sparseComponents[componentId.IncrementalId] = null;
            return true;
        }

        public bool TryGetValue(ComponentId componentId, out IComponentBase value)
        {
            if (_sparseComponents.Length <= componentId.IncrementalId ||
                _sparseComponents[componentId.IncrementalId] == null)
            {
                value = null;
                return false;
            }

            value = _sparseComponents[componentId.IncrementalId];
            return true;
        }

        public IComponentBase this[ComponentId componentId]
        {
            get => _sparseComponents[componentId.IncrementalId];
            set => Add(componentId, value);
        }

        public ICollection<ComponentId> Keys { get; }
        public ICollection<IComponentBase> Values => _denseComponents;
    }


    public class Entity : IEntity
    {
        public IWorld World { get; }
        public void Del(IComponentBase component)
        {
            
        }

        public IDictionary<ComponentId, IComponentBase> Components { get; }

        private BigInteger _componentMask;
        public Euid Euid { get; }
        
        
        public void Destroy()
        {
            // foreach (var component in Components)
            // {
            //     component.Value.Destroy();
            // }
            World.OnEntityDestroyed(this);
        }

        public BigInteger GetComponentsEntityMask() => _componentMask;
        public IEnumerable<IFilter> Filters() => _filters.Values;

        public Entity(IWorld world, string name)
        {
            World = world;
            Euid = IdProvider.CreateEuid(world, name);
            Components = new ComponentsDictionary();
            world.OnEntityCreated(this);
        }

        public T Get<T>() where T : IComponentBase
        {
            var id = IdProvider.GetIdByType<T>();
            var component = Components[id];
            if (component == null) throw new Exception($"Component {typeof(T)} does not exist on entity {Euid}");
            return (T)component;
        }
        

    
        private T AddInternal<T>(T component) where T : IComponentBase
        {
            var id = IdProvider.GetIdByType<T>();
            if (Components.TryGetValue(id, out var outComponent))
            {
                return (T)outComponent;
            }
        
            component.Entity = this;
            Components.Add(id, component);
            _componentMask |= id.MaskId;
            World.OnComponentAdded(this, component, id);
            return component;
        }

        public T Add<T>(T component) where T : IComponentBase
        {
            var componentInterfaces = component
                .GetType()
                .GetInterfaces();
                // .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IComponent<>));
      
            foreach (var componentInterface in componentInterfaces)
            {
                Type componentType = null;
                if (componentInterface == typeof(IComponentBase))
                {
                    componentType = component.GetType();
                }
                else if (typeof(IComponentBase).IsAssignableFrom(componentInterface))
                {
                    componentType = componentInterface;
                }
                else
                {
                    continue;
                }
                
                Type[] typeArgs = { componentType };
                var method = typeof(Entity).GetMethod("AddInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                var generic = method.MakeGenericMethod(typeArgs);
                generic.Invoke(this, new object[] { component });
            }

            return component;
        }

        public void Del<T>() where T : IComponentBase
        {
            var id = IdProvider.GetIdByType<T>();
            if (!Components.Remove(id, out var component)) return;
            var removeIdMask = ~id.MaskId;
            _componentMask &= removeIdMask;
            World.OnComponentDeleted(this, component, id);
        }

        public bool Has<T>() where T : IComponentBase
        {
            var id = IdProvider.GetIdByType<T>();
            return Components.ContainsKey(id);
        }


        private Dictionary<int, IFilter> _filters = new();
        public void OnAddInFilter(IFilter filter)
        {
            _filters.TryAdd(filter.FilterId.Id, filter);
        }

        public void OnRemoveFromFilter(IFilter filter)
        {
            _filters.Remove(filter.FilterId.Id);
        }
    }
}

/*public class Entity2 : IDisposable
{
    private World World;
    public Guid Guid { get; private set; }
    private Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();
    public Entity(World world)
    {
        World = world;
        Guid = Guid.NewGuid();
    }

    public T Get<T>() where T : IComponent, new()
    {
        if (_components.TryGetValue(typeof(T), out var component))
            return (T) component;

        var newComponent = new T();
        World.AddComponent(this, newComponent);
        _components.Add(typeof(T), newComponent);
        return newComponent;
    }
    
    public void Del<T>() where T : IComponent
    {
        if (!_components.TryGetValue(typeof(T), out var component))
            return;
        
        World.RemoveComponent(this, component);
        _components.Remove(typeof(T));
    }

    public void Dispose()
    {
        
    }

    public void Command(string name, object command)
    {
        
    }
}*/