using System.Numerics;

namespace Kobanan;



public struct TypeId
{
    //public readonly int Id;
    public readonly BigInteger Id;
    public TypeId(BigInteger id)
    {
        Id = id;
    }
}

public static class StaticTypeId<T>
{
    public static int Id { get; set; }
    private static int _lastId;
    
    static StaticTypeId()
    {
        _lastId++;
        Id = _lastId;
    }
}
public static class IdProvider
{
    public static int GetTypeId<T>()
    {
        return StaticTypeId<T>.Id;
    }

    public static Euid CreateEuid(IWorld world, string name)
    {
        return new Euid($"{world.Name}:{name}:{Guid.NewGuid()}");
    }
}