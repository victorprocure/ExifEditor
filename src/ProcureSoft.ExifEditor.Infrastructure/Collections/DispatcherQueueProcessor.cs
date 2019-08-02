using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProcureSoft.ExifEditor.Infrastructure.Collections
{
    internal sealed class DispatcherQueueProcessor : IDisposable
    {
        private static readonly Lazy<DispatcherQueueProcessor> _instance = new Lazy<DispatcherQueueProcessor>(() => new DispatcherQueueProcessor(), true);

        private readonly BlockingCollection<Action> _actionQueue;
        private readonly object _actionWaitingLock = new object();
        private readonly Semaphore _actionWaitingSemaphore = new Semaphore(0, 1);
        private readonly System.Timers.Timer _appStartTimer;
        private readonly object _startQueueLock = new object();
        private Action _actionWaiting;
        private Dispatcher _dispatcher;
        private ConcurrentDictionary<WeakReference, object> _subscriberQueue;

        private DispatcherQueueProcessor()
        {
            _actionQueue = new BlockingCollection<Action>();
            _subscriberQueue = new ConcurrentDictionary<WeakReference, object>();
            if (!CheckIfDispatcherCreated())
            {
                _appStartTimer = new System.Timers.Timer(100);
                _appStartTimer.Elapsed += (sender, e) =>
                {
                    if (CheckIfDispatcherCreated())
                    {
                        _appStartTimer.Enabled = false;
                    }
                };
                _appStartTimer.Enabled = true;
            }
        }

        public static DispatcherQueueProcessor Instance => _instance.Value;

        public bool IsDispatcherThread => CheckIfDispatcherCreated() && _dispatcher.CheckAccess();

        public void Add(Action Action)
        {
            if (!IsDispatcherThread)
            {
                _actionQueue.Add(Action);
            }
            else
            {
                lock (_actionWaitingLock)
                {
                    _actionQueue.Add(Action);

                    if (_actionWaiting != null)
                    {
                        _actionWaiting();
                        _actionWaiting = null;
                    }

                    var CountDown = 300;

                    Action NextCommand = null;
                    while (CountDown > 0 && _actionQueue.TryTake(out NextCommand))
                    {
                        --CountDown;
                        NextCommand();
                    }
                }
            }
        }

        public void Dispose()
        {
            _appStartTimer?.Dispose();
            _actionQueue?.Dispose();
            _actionWaitingSemaphore?.Dispose();
        }

        public IDisposable QueueSubscribe(Action SubscribeAction)
        {
            if (_subscriberQueue != null)
            {
                try
                {
                    var WeakReference = new WeakReference(SubscribeAction);
                    _subscriberQueue[WeakReference] = null;

                    return new DisposeDelegate(SubscribeAction, () =>
                    {
                        var subscriberQueue = _subscriberQueue;
                        subscriberQueue?.TryRemove(WeakReference, out var Dummy);
                    });
                }
                catch
                {
                    if (_subscriberQueue == null)
                        SubscribeAction();
                }
            }
            else
            {
                SubscribeAction();
            }

            return new DisposeDelegate();
        }

        private bool CheckIfDispatcherCreated()
        {
            if (_dispatcher != null)
            {
                return true;
            }
            else
            {
                lock (_startQueueLock)
                {
                    if (_dispatcher == null && Application.Current != null)
                    {
                        _dispatcher = Application.Current.Dispatcher;
                        _dispatcher.ShutdownStarted += (s, e) => _dispatcher = null;
                        StartQueueProcessing();
                    }
                    return _dispatcher != null;
                }
            }
        }

        private void StartQueueProcessing()
        {
            var Keys = _subscriberQueue.Keys;
            _subscriberQueue = null;

            foreach (var i in Keys)
            {
                var subscribe = i.Target as Action;
                subscribe?.Invoke();
            }
            Keys = default;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (!_actionQueue.IsCompleted)
                    {
                        // Get action from queue in this background thread.
                        while (true)
                        {
                            lock (_actionWaitingLock)
                            {
                                try
                                {
                                    if (_actionQueue.TryTake(out var action, 1))
                                    {
                                        _actionWaiting = action;
                                        break;
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    // Do nothing
                                }
                            }

                            try
                            {
                                Thread.Sleep(1);
                            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                            {
                                // Do nothing
                            }
                        }
                        // Wait to join to the dispatcher thread
                        _dispatcher?.Invoke((Action)(() =>
                        {
                            lock (_actionWaitingLock)
                            {
                                // Action might have already been executed by UI thread in Add method.
                                if (_actionWaiting != null)
                                {
                                    _actionWaiting();
                                    _actionWaiting = null;
                                }
                                // Clear the more of the action queue, up to 100 items at a time.
                                // Batch up processing into lots of 100 so as to give some
                                // responsiveness if the collection is being bombarded.
                                var countDown = 100;
                                Action nextCommand = null;
                                // Note that countDown must be tested first, otherwise we throw away
                                // a queue item
                                while (countDown > 0 && _actionQueue.TryTake(out nextCommand))
                                {
                                    --countDown;
                                    nextCommand();
                                }
                            }
                        }));
                    }
                }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                {
                    // Do nothing
                }
            });
        }

        private sealed class DisposeDelegate : IDisposable
        {
            public Action subscribeAction = null;

            private Action _disposeAction = null;

            public DisposeDelegate()
            {
            }

            public DisposeDelegate(Action SubscribeAction, Action DisposeAction)
            {
                subscribeAction = SubscribeAction;
                _disposeAction = DisposeAction;
            }

            public void Dispose()
            {
                _disposeAction?.Invoke();
                _disposeAction = null;
            }
        }
    }
}