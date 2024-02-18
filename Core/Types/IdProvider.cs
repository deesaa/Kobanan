using System;
using System.Numerics;

namespace Kobanan
{

    public struct FilterMaskTypes
    {
        public Type[] IncludeTypes;
        public Type[] ExcludeTypes;
    }
    public static class IdProvider
    {
        public static BigInteger GetIdByType<T>()
        {
            return StaticTypeId<T>.Id;
        }
        
        public static Type GetTypeById(BigInteger id)
        {
            
        }
        
        public static FilterMaskTypes GetTypesByMask(FilterMask mask)
        {
            
        }
        

        public static Euid CreateEuid(IWorld world, string name)
        {
            return new Euid($"{world.Name}:::{name}:::{Guid.NewGuid()}");
        }
    }
}