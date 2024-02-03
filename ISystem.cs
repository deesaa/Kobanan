namespace Kobanan;

public interface ISystem
{
    public IWorld World { get; }
}

public class Filter<T> where T : IComponent
{
    
}