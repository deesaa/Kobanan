using System.Collections.Generic;
using System.Numerics;

namespace Kobanan
{
    public interface IWorld
    {
        IDictionary<Euid, IEntity> Entities { get; }
        string Name { get; }
        IFilterBuilder Filter<T>();
        IEntity Get(Euid id);
        IEntity NewEntity(string name);
        IEntity NewEntityWithEuid(Euid euid);
        void OnEntityCreated(IEntity entity);
        void AddSystem<T>(T system) where T : ISystem;
        void RemoveSystem<T>() where T : ISystem;
        void Update();
        void Destroy();
        void OnComponentAdded(IEntity entity, IComponentBase component, ComponentId componentId);
        void OnComponentDeleted(IEntity entity, IComponentBase component, ComponentId componentId);
        void OnEntityDestroyed(IEntity entity);
        T GetService<T>() where T : IService;
        void OnFilterCreated(IFilter filter);
    }
}