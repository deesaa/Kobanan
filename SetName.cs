
namespace Kobanan;

public struct SetName : IComponent<SetName>
{
    public string Name;
    public IEntity Entity { get; }
    public int TypeUid { get; }
    
}

/*public abstract class Component : IComponent
{
    public IEntity Entity { get; }
    public int TypeUid { get; }
}*/