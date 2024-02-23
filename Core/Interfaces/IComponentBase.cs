namespace Kobanan
{
    public interface IComponentBase
    {
        IEntity Entity { get; set; } // Owner entity
        IWorld World => Entity.World; // Owner entity
        ComponentId GetComponentId();
        void Destroy();
    }
}