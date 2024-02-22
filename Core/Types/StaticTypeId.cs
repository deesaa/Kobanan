using System;
using System.Numerics;

namespace Kobanan
{

    public struct ComponentId
    {
        public readonly int IncrementalId; // Used as index in arrays
        public readonly BigInteger MaskId; // Bit mask - power of 2

        public ComponentId(BigInteger maskId, int incrementalId)
        {
            MaskId = maskId;
            IncrementalId = incrementalId;
        }
    }
    
    public static class StaticTypeId<T>
    {
        public static ComponentId Id { get; set; }
        static StaticTypeId()
        {
            StaticTypeIdCounter.Next(out var outId);
            Id = outId;
        }
    }

   
}