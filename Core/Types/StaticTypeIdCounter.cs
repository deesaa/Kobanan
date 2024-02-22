using System.Numerics;

namespace Kobanan
{
    public static class StaticTypeIdCounter
    {
        private static BigInteger LastMaskId;
        private static int LastIncrementalId;
        
        static StaticTypeIdCounter()
        {
            LastMaskId = 1;
            LastIncrementalId = 0;
        }

        public static void Next(out ComponentId componentId)
        {
            componentId = new ComponentId(LastMaskId, LastIncrementalId);
            LastMaskId <<= 1;
            LastIncrementalId++;
        }
    }
}