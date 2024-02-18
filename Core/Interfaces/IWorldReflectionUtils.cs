using System;

namespace Kobanan
{
    public static class IWorldReflectionUtils
    {
        public static IService GetService(this IWorld world, Type type)
        {
            var generic = world.GetType().GetMethod("GetService");
            var instance = generic.MakeGenericMethod(type);
            return (IService)instance.Invoke(world, null);
        }
    }
}