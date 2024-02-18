using System;
using System.Collections.Generic;

namespace Kobanan
{
    public struct Euid : IEqualityComparer<Euid>
    {
        public readonly string Id;
        public readonly int Hash;
        public Euid(string guid)
        {
            Id = guid;
            Hash = Id.GetHashCode();
        }

        public bool Equals(Euid x, Euid y) => x.Hash == y.Hash && x.Id == y.Id;
        public int GetHashCode(Euid obj) => obj.Hash;
        public override int GetHashCode() => Hash;
    }
}