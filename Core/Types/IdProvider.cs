using System;
using System.Collections.Generic;
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
        public static List<Type> TypesMap = new();
        public static ComponentId GetIdByType<T>()
        {
            var cId = StaticTypeId<T>.Id;
            if(TypesMap.Count <= cId.IncrementalId)
                TypesMap.Add(typeof(T));
            return cId;
        }
        
        public static ComponentId GetIdByType(Type type)
        {
            var generic = typeof(StaticTypeId<>);
            var instance = generic.MakeGenericType(type);
            var idProperty = instance.GetProperty("Id");
            var id = (ComponentId)idProperty.GetValue(null);
            return id;
        }

        public static Type GetTypeById(ComponentId id) => TypesMap[id.IncrementalId];
    
        public static FilterMaskTypes GetTypesByMask(FilterMask mask)
        {
            return default;
        }

        public static Euid CreateEuid(IWorld world, string name)
        {
            return new Euid($"{world.Name}:::{name}:::{Guid.NewGuid()}");
        }
    }
}