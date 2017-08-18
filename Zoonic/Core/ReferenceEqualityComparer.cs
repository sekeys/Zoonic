namespace Zoonic.Concurrency
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class ReferenceEqualityComparer
        : IEqualityComparer, IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Default = new ReferenceEqualityComparer();

        ReferenceEqualityComparer()
        {
        }

        public new bool Equals(object x, object y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}