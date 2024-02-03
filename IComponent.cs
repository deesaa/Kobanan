namespace Kobanan;

public interface IComponentBase
{
    IEntity Entity { get; set; } // Owner entity
    IWorld World => Entity.World; // Owner entity
   // TypeId TypeUid { get; }
}

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

public interface IComponent<T> : IComponentBase
{
   // public static TypeId TypeId => new(IdProvider.GetTypeId<T>());
}