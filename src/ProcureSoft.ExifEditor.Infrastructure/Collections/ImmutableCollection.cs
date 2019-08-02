using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    public abstract class ImmutableCollection<T> : ICollection, ICollection<T>
    {
        public abstract int Count { get; }

        public bool IsReadOnly => true;

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        public void Add(T item) => throw new NotSupportedException("KeyCollection<TKey,TValue> is read-only.");

        public void Clear() => throw new NotSupportedException("KeyCollection<TKey,TValue> is read-only.");

        public abstract bool Contains(T item);

        void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(T item) => throw new NotSupportedException("KeyCollection<TKey,TValue> is read-only.");
    }
}