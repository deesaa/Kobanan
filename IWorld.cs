using System.Collections;
using System.Numerics;

namespace Kobanan;

public class World : IWorld
{
    private Dictionary<Euid, IEntity> _entities = new Dictionary<Euid, IEntity>();
    public IDictionary<Euid, IEntity> Entities => _entities;
    public string Name { get; }
    public World(string name = "Default")
    {
        Name = name;
    }

    public IFilterBuilder Filter<T>()
    {
        
    }

    public IEntity Get(Euid id)
    {
       
    }
    
    public IEntity NewEntity(string name)
    {
        var newE = new Entity(this, name);
        return newE;
    }
    
    public IEntity NewEntityWithEuid(Euid euid)
    {
        
    }
    
    public void OnEntityCreated(IEntity entity)
    {
        if (!Entities.TryAdd(entity.Euid, entity))
            throw new Exception($"Entity with Euid:{entity.Euid} already exists in world:{Name}");
    }

    public void AddSystem(ISystem system)
    {
        
    }

    public void RemoveSystem(ISystem system)
    {
        
    }

    public void Update()
    {
        
    }

    public void Destroy()
    {
        
    }

    public void OnComponentCreated(IEntity entity, IComponentBase component, TypeId id)
    {
        
    }

    public void OnComponentDeleted(IEntity entity, IComponentBase component, TypeId id)
    {
        
    }
}

public interface IWorld
{
    IDictionary<Euid, IEntity> Entities { get; }
    string Name { get; }
    IFilterBuilder Filter<T>();
    IEntity Get(Euid id);
    IEntity NewEntity(string name);
    IEntity NewEntityWithEuid(Euid euid);
    void OnEntityCreated(IEntity entity);
    void AddSystem(ISystem system);
    void RemoveSystem(ISystem system);
    void Update();
    void Destroy();
    void OnComponentCreated(IEntity entity, IComponentBase component, TypeId typeId);
    void OnComponentDeleted(IEntity entity, IComponentBase component, TypeId typeId);
}

public interface IFilter : IEnumerable<IEntity>
{
    
}

public class Filter : IFilter
{
    public IEnumerator<IEntity> GetEnumerator()
    {
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        
    }
}

public class FilterBuilder : IFilterBuilder
{
    private IFilter _filter;
    private IWorld _world;

    public FilterBuilder(IWorld world)
    {
        _filter = new Filter();
        _world = world;
    }
    public IFilter End()
    {
        BigInteger g = BigInteger.Compare();
    }

    public IFilterBuilder Inc<T>()
    {
       
    }

    public IFilterBuilder Exc<T>()
    {
        
    }

    public IFilterBuilder Des<T>()
    {
       
    }

    public IFilterBuilder Child<T>()
    {
        
    }
}

public interface IFilterBuilder
{
    IFilter End();
    IFilterBuilder Inc<T>();
    IFilterBuilder Exc<T>();
    IFilterBuilder Des<T>();
    IFilterBuilder Child<T>();
}