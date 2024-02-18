using System.Numerics;

namespace Kobanan
{
    public static class StaticTypeIdCounter
    {
        private static BigInteger LastId;
        
        static StaticTypeIdCounter()
        {
            LastId = 1;
        }

        public static void Next(out BigInteger id)
        {
            id = LastId;
            LastId <<= 1;
        }
    }
}