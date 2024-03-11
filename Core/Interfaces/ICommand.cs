namespace Kobanan
{
    public abstract class Command : IComponentBase
    {
        public IEntity Entity { get; set; }
    }
}       