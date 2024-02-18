using System;
using System.Numerics;

namespace Kobanan
{
    public static class StaticTypeId<T>
    {
        public static BigInteger Id { get; set; }
        static StaticTypeId()
        {
            StaticTypeIdCounter.Next(out var outId);
            Id = outId;
        }
    }

    public static class ReflectionStaticTypeId
    {
        public static BigInteger Get(Type type)
        {
            var generic = typeof(StaticTypeId<>);
            var instance = generic.MakeGenericType(type);
            var idProperty = instance.GetProperty("Id");
            var id = (BigInteger)idProperty.GetValue(null);
            return id;
        }
    }
}