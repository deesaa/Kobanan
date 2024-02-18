using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Unity.VisualScripting;

namespace Kobanan
{
    public class Entity : IEntity
    {
        public IWorld World { get; }
        public void Del(IComponentBase component)
        {
            
        }

        private BigInteger _componentMask;
        public IDictionary<BigInteger, IComponentBase> Components { get; set; }
        public Euid Euid { get; }
        
        
        public void Destroy()
        {
            foreach (var component in Components)
            {
                component.Value.Destroy();
            }
            World.OnEntityDestroyed(this);
        }

        public BigInteger GetComponentMask() => _componentMask;

        public Entity(IWorld world, string name)
        {
            World = world;
            Euid = IdProvider.CreateEuid(world, name);
            Components = new Dictionary<BigInteger, IComponentBase>();
            world.OnEntityCreated(this);
        }

        public T Get<T>() where T : IComponent<T>
        {
            var id = IdProvider.GetIdByType<T>();
            if (!Components.TryGetValue(id, out var component))
                throw new Exception($"Component {typeof(T)} does not exist on entity {Euid}");
            return (T)component;
        }
        

    
        public T Add<T>(T component) where T : IComponent<T>
        {
            var id = IdProvider.GetIdByType<T>();
            if (Components.TryGetValue(id, out var outComponent))
            {
                return (T)outComponent;
            }
        
            component.Entity = this;
            Components.Add(id, component);
            _componentMask |= id;
            World.OnComponentCreated(this, component, id);
            return component;
        }

        public void AddBase(IComponentBase component)
        {
            var componentInterfaces = component
                .GetType()
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IComponent<>));
      
            foreach (var componentInterface in componentInterfaces)
            {
                var interfaceGenericArg = componentInterface.GenericTypeArguments[0];
                Type[] typeArgs = { interfaceGenericArg };
                var method = typeof(Entity).GetMethod("Add");
                var generic = method.MakeGenericMethod(typeArgs);
                generic.InvokeOptimized(this, component);
            }
        }

        public void Del<T>() where T : IComponent<T>
        {
            var id = IdProvider.GetIdByType<T>();
            if (!Components.Remove(id, out var component)) return;
            var removeIdMask = ~id;
            _componentMask &= removeIdMask;
            World.OnComponentDeleted(this, component, id);
        }

        public bool Has<T>() where T : IComponent<T>
        {
            var id = IdProvider.GetIdByType<T>();
            return Components.ContainsKey(id);
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