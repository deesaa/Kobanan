using System;

namespace Kobanan
{
    public static class IEntityReflectionUtils
    {
        public static IComponentBase Get(this IEntity entity, Type type)
        {
            var generic = entity.GetType().GetMethod("Add");
            var instance = generic.MakeGenericMethod(type);
            return (IComponentBase)instance.Invoke(entity, null);
        }
    }
}