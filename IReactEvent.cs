namespace Kobanan
{
    public interface IReactEvent<T> where T : IEvent
    {
        public void OnEvent(T @event);
    }
}