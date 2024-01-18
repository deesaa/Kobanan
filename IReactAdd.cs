namespace Kobanan;

public interface IReactAdd<T> where T : IComponent
{
    public void OnAdd(T component);
}