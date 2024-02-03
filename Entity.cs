namespace Kobanan;

public class Entity : IEntity
{
    public IWorld World { get; }
    public IDictionary<int, IComponentBase> Components { get; set; }
    public Euid Euid { get; }

    public Entity(IWorld world, string name)
    {
        World = world;
        Euid = IdProvider.CreateEuid(world, name);
        world.OnEntityCreated(this);
    }

    public T Get<T>() where T : IComponent<T>
    {
        int id = IdProvider.GetTypeId<T>();
        if (!Components.TryGetValue(id, out var component))
            return default;
        return (T)component;
    }
    
    public T Add<T>(T component) where T : IComponent<T>
    {
        int id = IdProvider.GetTypeId<T>();
        if (Components.TryGetValue(id, out var outComponent))
        {
            return (T)outComponent;
        }
        
        component.Entity = this;
        Components.Add(id, component);
        World.OnComponentCreated(this, component, id);
        return component;
    }
    
    public void Del<T>() where T : IComponent<T>, new()
    {
        int id = IdProvider.GetTypeId<T>();
        if (!Components.Remove(id, out var component)) return;
        World.OnComponentDeleted(this, component, id);
    }

    public bool Has<T>() where T : IComponent<T>
    {
        int id = IdProvider.GetTypeId<T>();
        return Components.ContainsKey(id);
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