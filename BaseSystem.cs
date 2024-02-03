namespace Kobanan;

public abstract class BaseSystem : ISystem
{
    public IWorld World { get; private set; }
}