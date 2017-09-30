using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Broadcast
{
    public class DefaultBroadcast : Broadcast
    {
        private static ConcurrentDictionary<Type, List<ISubscription>> _listeners =
            new ConcurrentDictionary<Type, List<ISubscription>>();
        

        public override void Publish<T>(T message)
        {
            foreach (var item in this.GetSubscribersByMessage<T>())
            {
                item.Run(BroadcastEvent<T>.Create(message));
            }
        }

        public override void Publish(object message)
        {
            if (message == null) { return; }
            foreach (var item in this.GetSubscribersByMessage(message.GetType()))
            {
                item.Run(BroadcastEvent.Create(message));
            }
        }
        public override async Task PublishAsync(object message)
        {
            if (message == null) { return; }
            foreach (var item in this.GetSubscribersByMessage(message.GetType()))
            {
                await Task.Run(() => { item.Run(BroadcastEvent.Create(message)); });
            }
        }

        public override async Task PublishAsync<T>(T message)
        {
            foreach (var item in this.GetSubscribersByMessage<T>())
            {
                await Task.Run(() => { item.Run(BroadcastEvent<T>.Create(message)); });
            }
        }
        private IEnumerable<ISubscription> GetSubscribersByMessage<T>()
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
            {
                return Enumerable.Empty<ISubscription>();
            }
            return _listeners[type];
        }
        private IEnumerable<ISubscription> GetSubscribersByMessage(Type type)
        {
            if (!_listeners.ContainsKey(type))
            {
                return Enumerable.Empty<ISubscription>();
            }
            return _listeners[type];
        }

        public override void Subscribe<T>(ISubscription<T> subscription)
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
            {
                _listeners.TryAdd(type, new List<ISubscription>());
            }
            _listeners[type].Add(subscription);

        }



        public override void Unsubscribe(string eventKey)
        {
            foreach (var kv in _listeners)
            {
                for (var i = 0; i < kv.Value.Count; i++)
                {
                    if (string.Equals(kv.Value[i].EventKey, eventKey))
                    {
                        kv.Value.Remove(kv.Value[i]);
                    }
                }
            }
        }



        public override void Publish<T>(IBroadcastEvent<T> evnt)
        {
            if (evnt == null) { return; }
            foreach (var item in this.GetSubscribersByMessage<T>())
            {
                item.Run(evnt);
            }
        }

        public override async Task PublishAsync<T>(IBroadcastEvent<T> evnt)
        {
            if (evnt == null) { return; }
            foreach (var item in this.GetSubscribersByMessage<T>())
            {
                await Task.Run(() => { item.Run(evnt); });
            }
        }

        public override async Task PublishAsync(IBroadcastEvent evnt)
        {
            if (evnt == null) { return; }
            var type = evnt.GetContent().GetType();
            foreach (var item in this.GetSubscribersByMessage(type))
            {
                await Task.Run(() => { item.Run(evnt); });
            }
        }

        public override void Publish(IBroadcastEvent evnt)
        {
            if (evnt == null) { return; }
            var type = evnt.GetContent().GetType();
            foreach (var item in this.GetSubscribersByMessage(type))
            {
                item.Run(evnt);
            }
        }

        public override List<ISubscription> Unsubscribe<T>()
        {
            List<ISubscription> list = new List<ISubscription>();
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
            {
                return list;
            }
            _listeners.TryRemove(type, out list);
            return list;
        }
    }
}
