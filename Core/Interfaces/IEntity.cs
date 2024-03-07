using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Kobanan
{
    public interface IEntity
    {
        IWorld World { get; }
        T Get<T>() where T : IComponentBase;
        T Add<T>(T component) where T : IComponentBase;
        bool Has<T>() where T : IComponentBase;
        void Del<T>() where T : IComponentBase;
        void Del(IComponentBase component);
        IDictionary<ComponentId, IComponentBase> Components { get; }
        Euid Euid { get; }
        void Destroy();
        BigInteger GetComponentsEntityMask();
        IEnumerable<IFilter> Filters();
        void OnAddInFilter(IFilter filter);
        void OnRemoveFromFilter(IFilter filter);
    }
}