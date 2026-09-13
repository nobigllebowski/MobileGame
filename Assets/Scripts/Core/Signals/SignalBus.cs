using System;
using System.Collections.Generic;

namespace Nation.Core.Signals
{
    /// <summary>
    /// Typed publish/subscribe bus. Publishing allocates nothing: handlers for each signal type are kept
    /// in an array that is rebuilt only when a subscription is added or removed, so a handler may safely
    /// unsubscribe while a publish is in progress.
    /// </summary>
    public sealed class SignalBus
    {
        private readonly Dictionary<Type, ISubscriptionList> _lists = new Dictionary<Type, ISubscriptionList>();

        public IDisposable Subscribe<TSignal>(Action<TSignal> handler) where TSignal : ISignal
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var list = GetOrCreateList<TSignal>();
            list.Add(handler);
            return new Subscription<TSignal>(list, handler);
        }

        public void Publish<TSignal>(TSignal signal) where TSignal : ISignal
        {
            if (_lists.TryGetValue(typeof(TSignal), out var list))
            {
                ((SubscriptionList<TSignal>)list).Invoke(signal);
            }
        }

        public int SubscriberCount<TSignal>() where TSignal : ISignal
        {
            return _lists.TryGetValue(typeof(TSignal), out var list) ? list.Count : 0;
        }

        private SubscriptionList<TSignal> GetOrCreateList<TSignal>() where TSignal : ISignal
        {
            if (!_lists.TryGetValue(typeof(TSignal), out var list))
            {
                list = new SubscriptionList<TSignal>();
                _lists.Add(typeof(TSignal), list);
            }

            return (SubscriptionList<TSignal>)list;
        }

        private interface ISubscriptionList
        {
            int Count { get; }
        }

        private sealed class SubscriptionList<TSignal> : ISubscriptionList where TSignal : ISignal
        {
            private static readonly Action<TSignal>[] Empty = new Action<TSignal>[0];

            private Action<TSignal>[] _handlers = Empty;

            public int Count => _handlers.Length;

            public void Add(Action<TSignal> handler)
            {
                var next = new Action<TSignal>[_handlers.Length + 1];
                Array.Copy(_handlers, next, _handlers.Length);
                next[_handlers.Length] = handler;
                _handlers = next;
            }

            public void Remove(Action<TSignal> handler)
            {
                var index = Array.IndexOf(_handlers, handler);
                if (index < 0)
                {
                    return;
                }

                if (_handlers.Length == 1)
                {
                    _handlers = Empty;
                    return;
                }

                var next = new Action<TSignal>[_handlers.Length - 1];
                Array.Copy(_handlers, 0, next, 0, index);
                Array.Copy(_handlers, index + 1, next, index, _handlers.Length - index - 1);
                _handlers = next;
            }

            public void Invoke(TSignal signal)
            {
                var handlers = _handlers;
                for (var i = 0; i < handlers.Length; i++)
                {
                    handlers[i](signal);
                }
            }
        }

        private sealed class Subscription<TSignal> : IDisposable where TSignal : ISignal
        {
            private SubscriptionList<TSignal> _list;
            private Action<TSignal> _handler;

            public Subscription(SubscriptionList<TSignal> list, Action<TSignal> handler)
            {
                _list = list;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_list == null)
                {
                    return;
                }

                _list.Remove(_handler);
                _list = null;
                _handler = null;
            }
        }
    }
}
