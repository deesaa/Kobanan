using System.Numerics;

namespace Kobanan
{
    public struct TypeId
    {
        //public readonly int Id;
        public readonly BigInteger Id;
        public TypeId(BigInteger id)
        {
            Id = id;
        }
    }
}