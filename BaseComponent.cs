namespace Kobanan;

public abstract class BaseComponent : IComponent
{
    public Entity Entity { get; private set; }
}