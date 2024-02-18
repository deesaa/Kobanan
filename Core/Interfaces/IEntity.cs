using System.Collections.Generic;
using System.Numerics;

namespace Kobanan
{
    public interface IEntity
    {
        IWorld World { get; }
        T Get<T>() where T : IComponent<T>;
        T Add<T>(T component) where T : IComponent<T>;
        void AddBase(IComponentBase component);
        bool Has<T>() where T : IComponent<T>;
        void Del<T>() where T : IComponent<T>;
        void Del(IComponentBase component);
        IDictionary<BigInteger, IComponentBase> Components { get; set; }
        Euid Euid { get; }
        void Destroy();
        BigInteger GetComponentMask();
    }
}