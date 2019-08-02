using System.Collections.Generic;
using System.Linq;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    public sealed class ImmutableCollectionImpl<T> : ImmutableCollection<T>
    {
        private readonly IList<T> _source;

        public ImmutableCollectionImpl(IEnumerable<T> source) => _source = new List<T>(source);

        public ImmutableCollectionImpl() : this(Enumerable.Empty<T>())
        {
        }

        public override int Count => _source.Count;

        public override bool Contains(T item) => _source.Contains(item);

        public override void CopyTo(T[] array, int arrayIndex) => _source.CopyTo(array, arrayIndex);

        public override IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
    }
}