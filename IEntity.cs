namespace Kobanan;

public interface IEntity
{
    IWorld World { get; }
    T Get<T>() where T : IComponent<T>;
    T Add<T>(T component) where T : IComponent<T>;
    bool Has<T>() where T : IComponent<T>;
    IDictionary<int, IComponentBase> Components { get; set; }
    Euid Euid { get; }
}

public struct Euid
{
    public readonly string Id;
    public Euid(string guid)
    {
        Id = guid;
    }
}