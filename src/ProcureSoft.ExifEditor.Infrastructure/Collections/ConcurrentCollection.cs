using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    [Serializable]
    public abstract class ConcurrentCollection<T> : IObservable<NotifyCollectionChangedEventArgs>, INotifyCollectionChanged, IEnumerable<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _snapshotLock = new ReaderWriterLockSlim();
        private readonly Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>> _subscribers;
        private ImmutableCollection<T> _baseSnapshot;
        private bool _isDisposed;
        private bool _newSnapshotRequired;
        private int _subscriberKey;

        protected ConcurrentCollection() : this(new T[] { })
        {
        }

        protected ConcurrentCollection(IEnumerable<T> Items)
        {
            _subscribers = new Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>>();

            WriteCollection = new ObservableCollection<T>(Items);
            _baseSnapshot = new ImmutableCollectionImpl<T>(Items);

            ViewModel = new ConcurrentCollectionViewModel<T>(this);

            WriteCollection.CollectionChanged += HandleBaseCollectionChanged;

            ViewModel.CollectionChanged += (sender, e) => CollectionChanged?.Invoke(sender, e);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ImmutableCollection<T> Snapshot => DoBaseRead(() =>
                                                                        {
                                                                            UpdateSnapshot();
                                                                            return _baseSnapshot;
                                                                        });

        protected ObservableCollection<T> ReadCollection => GetIsDispatcherThread() ? ViewModel : WriteCollection;

        protected ConcurrentCollectionViewModel<T> ViewModel { get; }

        protected ObservableCollection<T> WriteCollection { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator() => GetIsDispatcherThread() ? ViewModel.GetEnumerator() : Snapshot.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            return DoBaseWrite(() =>
            {
                var Key = _subscriberKey++;

                _subscribers.Add(Key, observer);
                UpdateSnapshot();

                foreach (var i in _baseSnapshot)
                    observer.OnNext(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, i));

                return new DisposeDelegate(() => DoBaseWrite(() => _subscribers.Remove(Key)));
            });
        }

        protected static bool GetIsDispatcherThread() => DispatcherQueueProcessor.Instance.IsDispatcherThread;

        protected virtual void Dispose(bool Disposing)
        {
            OnCompleted();
            if (Disposing)
            {
                ViewModel?.Dispose();
                _readWriteLock?.Dispose();
                _snapshotLock?.Dispose();
            }
            _isDisposed = true;
        }

        protected void DoBaseClear(Action Action = null)
        {
            // Need a special case of DoBaseWrite for a set changes to make sure that nothing else
            // does a change while we are in the middle of doing a collection of changes.
            _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
            try
            {
                _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
                Action?.Invoke();
                while (WriteCollection.Count > 0)
                {
                    _newSnapshotRequired = true;
                    WriteCollection.RemoveAt(WriteCollection.Count - 1);
                }
            }
            finally
            {
                if (_readWriteLock.IsWriteLockHeld)
                    _readWriteLock.ExitWriteLock();
                _readWriteLock.ExitUpgradeableReadLock();
            }
        }

        protected void DoBaseRead(Action readFunc) => DoBaseRead<object>(() =>
                                                    {
                                                        readFunc();
                                                        return null;
                                                    });

        protected TResult DoBaseRead<TResult>(Func<TResult> ReadAction)
        {
            if (GetIsDispatcherThread())
                return ReadAction();

            _readWriteLock.TryEnterReadLock(Timeout.Infinite);

            try
            {
                return ReadAction();
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
        }

        protected TResult DoBaseReadWrite<TResult>(Func<bool> ReadFuncTest, Func<TResult> ReadFunc, Func<TResult> WriteFunc)
        {
            _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
            try
            {
                if (ReadFuncTest())
                {
                    return ReadFunc();
                }
                else
                {
                    _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
                    try
                    {
                        _newSnapshotRequired = true;
                        return WriteFunc();
                    }
                    finally
                    {
                        if (_readWriteLock.IsWriteLockHeld)
                            _readWriteLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _readWriteLock.ExitUpgradeableReadLock();
            }
        }

        protected TResult DoBaseReadWrite<TResult>(Func<bool> ReadFuncTest, Func<TResult> ReadFunc, Action PreWriteFunc, Func<TResult> WriteFunc)
        {
            _readWriteLock.TryEnterReadLock(Timeout.Infinite);
            try
            {
                if (ReadFuncTest())
                    return ReadFunc();
            }
            finally
            {
                _readWriteLock.ExitReadLock();
            }
            PreWriteFunc();
            return DoBaseReadWrite(ReadFuncTest, ReadFunc, WriteFunc);
        }

        protected void DoBaseWrite(Action WriteAction) => DoBaseWrite<object>(() =>
                                                        {
                                                            WriteAction();
                                                            return null;
                                                        });

        protected TResult DoBaseWrite<TResult>(Func<TResult> WriteFunc)
        {
            _readWriteLock.TryEnterUpgradeableReadLock(Timeout.Infinite);
            try
            {
                _readWriteLock.TryEnterWriteLock(Timeout.Infinite);
                _newSnapshotRequired = true;
                return WriteFunc();
            }
            finally
            {
                if (_readWriteLock.IsWriteLockHeld)
                    _readWriteLock.ExitWriteLock();
                _readWriteLock.ExitUpgradeableReadLock();
            }
        }

        protected void HandleBaseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var actionTypeIsOk = e.Action != NotifyCollectionChangedAction.Reset;
            System.Diagnostics.Debug.Assert(actionTypeIsOk, "Reset called on concurrent observable collection. This shouldn't happen");
            OnNext(e);
        }

        protected void OnCompleted()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Observable<T>");

            foreach (var i in _subscribers.Select(kv => kv.Value))
                i.OnCompleted();
        }

        protected void OnError(Exception exception)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Observable<T>");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            foreach (var i in _subscribers.Select(kv => kv.Value))
                i.OnError(exception);
        }

        protected void OnNext(NotifyCollectionChangedEventArgs value)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Observable<T>");

            foreach (var i in _subscribers.Select(kv => kv.Value))
                i.OnNext(value);
        }

        private void UpdateSnapshot()
        {
            if (_newSnapshotRequired)
            {
                _snapshotLock.TryEnterWriteLock(Timeout.Infinite);
                if (_newSnapshotRequired)
                {
                    _baseSnapshot = new ImmutableCollectionImpl<T>(WriteCollection);
                    _newSnapshotRequired = false;
                }
                _snapshotLock.ExitWriteLock();
            }
        }

        private sealed class DisposeDelegate : IDisposable
        {
            private readonly Action _dispose;

            public DisposeDelegate(Action Dispose) => _dispose = Dispose;

            public void Dispose() => _dispose();
        }
    }
}