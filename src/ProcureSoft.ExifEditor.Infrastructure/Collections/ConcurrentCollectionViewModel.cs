using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    [Serializable]
    public sealed class ConcurrentCollectionViewModel<T> : ObservableCollection<T>, IObserver<NotifyCollectionChangedEventArgs>, IDisposable
    {
        private readonly IDisposable _subscriptionActionToken;

        private IDisposable _unsubscribeToken;

        public ConcurrentCollectionViewModel(IObservable<NotifyCollectionChangedEventArgs> Observable) => _subscriptionActionToken = DispatcherQueueProcessor.Instance.QueueSubscribe(() => _unsubscribeToken = Observable.Subscribe(this));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnCompleted()
        {
            //Nothing to do here
        }

        public void OnError(Exception error)
        {
            //Nothing
        }

        public void OnNext(NotifyCollectionChangedEventArgs value) => DispatcherQueueProcessor.Instance.Add(() => ProcessCommand(value));

        private void Dispose(bool IsDisposing)
        {
            if (IsDisposing)
            {
                _subscriptionActionToken?.Dispose();
                _unsubscribeToken?.Dispose();
            }
        }

        private void ProcessCommand(NotifyCollectionChangedEventArgs Command)
        {
            switch (Command.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var StartIndex = Command.NewStartingIndex;
                        if (StartIndex > -1)
                        {
                            foreach (var i in Command.NewItems)
                            {
                                InsertItem(StartIndex, (T)i);
                                ++StartIndex;
                            }
                        }
                        else
                        {
                            foreach (var i in Command.NewItems)
                            {
                                Add((T)i);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (T i in Command.OldItems)
                            Remove(i);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        var StartIndex = Command.OldStartingIndex;
                        foreach (var i in Command.NewItems)
                        {
                            this[StartIndex] = (T)i;
                            ++StartIndex;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }
    }
}