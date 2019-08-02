using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    [Serializable]
    public class ConcurrentCollectionImpl<T> : ConcurrentCollection<T>, IList, IList<T>
    {
        private bool _isEmpty = true;

        public ConcurrentCollectionImpl()
        {
        }

        public ConcurrentCollectionImpl(T item) => Add(item);

        public ConcurrentCollectionImpl(params T[] items) : base(items)
        {
        }

        public ConcurrentCollectionImpl(IEnumerable<T> items) : base(items)
        {
        }

        public ConcurrentCollectionImpl(params IEnumerable<T>[] items)
        {
            foreach (var i in items)
            {
                foreach (var j in i)
                    Add(j);
            }
        }

        public event EventHandler<T> ItemAdded;

        public event EventHandler<object> ItemInserted;

        public event EventHandler<T> ItemRemoved;

        public event EventHandler<EventArgs> ItemsChanged;

        public event EventHandler<EventArgs> ItemsCleared;

        public event EventHandler<EventArgs> PreviewItemsCleared;

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        public int Count => DoBaseRead(() => ReadCollection.Count);

        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                OnPropertyChanged("IsEmpty");
            }
        }

        bool IList.IsFixedSize => DoBaseRead(() => ((IList)ReadCollection).IsFixedSize);

        public bool IsReadOnly => DoBaseRead(() => ((ICollection<T>)ReadCollection).IsReadOnly);

        bool IList.IsReadOnly => DoBaseRead(() => ((IList)ReadCollection).IsReadOnly);

        bool ICollection.IsSynchronized => DoBaseRead(() => ((ICollection)ReadCollection).IsSynchronized);

        object ICollection.SyncRoot => DoBaseRead(() => ((ICollection)ReadCollection).SyncRoot);

        object IList.this[int index]
        {
            get => DoBaseRead(() => ((IList)ReadCollection)[index]);
            set => DoBaseWrite(() => ((IList)WriteCollection)[index] = value);
        }

        public T this[int index]
        {
            get => DoBaseRead(() => ReadCollection[index]);
            set => DoBaseWrite(() => WriteCollection[index] = value);
        }

        int IList.Add(object value)
        {
            var Result = DoBaseWrite(() => ((IList)WriteCollection).Add(value));
            OnItemAdded((T)value);
            OnItemsChanged();
            return Result;
        }

        public void Add(T item)
        {
            DoBaseWrite(() => WriteCollection.Add(item));
            OnItemAdded(item);
            OnItemsChanged();
        }

        public async Task BeginClear() => await Task.Run(Clear).ConfigureAwait(false);

        public void Clear()
        {
            OnPreviewItemsCleared();
            DoBaseClear(null);
            OnItemsCleared();
            OnItemsChanged();
        }

        bool IList.Contains(object value) => DoBaseRead(() => ((IList)ReadCollection).Contains(value));

        public bool Contains(T item) => DoBaseRead(() => ReadCollection.Contains(item));

        void ICollection.CopyTo(Array array, int index) => DoBaseRead(() => ((ICollection)ReadCollection).CopyTo(array, index));

        public void CopyTo(T[] array, int arrayIndex) => DoBaseRead(() =>
                                                       {
                                                           if (array.Length >= ReadCollection.Count)
                                                           {
                                                               ReadCollection.CopyTo(array, arrayIndex);
                                                           }
                                                           else
                                                           {
                                                               Console.Out.WriteLine("ConcurrentObservableCollection attempting to copy into wrong sized array (array.Count < ReadCollection.Count)");
                                                           }
                                                       });

        int IList.IndexOf(object value) => DoBaseRead(() => ((IList)ReadCollection).IndexOf(value));

        public int IndexOf(T item) => DoBaseRead(() => ReadCollection.IndexOf(item));

        void IList.Insert(int index, object value)
        {
            DoBaseWrite(() => ((IList)WriteCollection).Insert(index, value));
            OnItemInserted((T)value, index);
            OnItemsChanged();
        }

        public void Insert(int index, T item)
        {
            DoBaseWrite(() => WriteCollection.Insert(index, item));
            OnItemInserted(item, index);
            OnItemsChanged();
        }

        public virtual void OnPropertyChanged(string Name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));

        void IList.Remove(object value)
        {
            DoBaseWrite(() => ((IList)WriteCollection).Remove(value));
            OnItemRemoved((T)value);
            OnItemsChanged();
        }

        public bool Remove(T item)
        {
            var Result = DoBaseWrite(() => WriteCollection.Remove(item));
            OnItemRemoved(item);
            OnItemsChanged();
            return Result;
        }

        void IList.RemoveAt(int index)
        {
            var Item = this[index];
            DoBaseWrite(() => ((IList)WriteCollection).RemoveAt(index));
            OnItemRemoved(Item);
            OnItemsChanged();
        }

        public void RemoveAt(int index)
        {
            var Item = this[index];
            DoBaseWrite(() => WriteCollection.RemoveAt(index));
            OnItemRemoved(Item);
            OnItemsChanged();
        }

        protected virtual void OnItemAdded(T item) => ItemAdded?.Invoke(this, item);

        protected virtual void OnItemInserted(T item, int index) => ItemInserted?.Invoke(this, (item, index));

        protected virtual void OnItemRemoved(T item) => ItemRemoved?.Invoke(this, item);

        protected virtual void OnItemsChanged()
        {
            OnPropertyChanged("Count");
            IsEmpty = Count == 0;
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnItemsCleared() => ItemsCleared?.Invoke(this, EventArgs.Empty);

        protected virtual void OnPreviewItemsCleared() => PreviewItemsCleared?.Invoke(this, EventArgs.Empty);
    }
}