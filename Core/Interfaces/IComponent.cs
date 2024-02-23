namespace Kobanan
{
    /*public abstract class BaseComponent<T> : IComponentBase
{
   // private static readonly TypeId _typeId;
    public IEntity Entity { get; set; }
 //   public TypeId TypeUid => _typeId;

    static BaseComponent()
    {
     //   var id = IdProvider.GetTypeId<T>();
     //   _typeId = new TypeId(id);
    }

    protected BaseComponent(IEntity entity)
    {
        Entity = entity;
    }
}*/

    public interface IComponent<T> : IComponentBase where T : IComponent<T>
    {
        ComponentId IComponentBase.GetComponentId()
        {
            return IdProvider.GetIdByType<T>();
        }
        
        void IComponentBase.Destroy()
        {
            Entity.Del<T>();
        }
    }
}