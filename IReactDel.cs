namespace Kobanan;

public interface IReactDel<T> where T : IComponent
{
    public void OnDel(T component);
}