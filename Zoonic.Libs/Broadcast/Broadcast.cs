using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Broadcast
{
    public abstract class Broadcast
    {
        private static Broadcast _Instance;

        public abstract void Subscribe<T>(ISubscription<T> subscription);
        public abstract void Publish<T>(T message) ;
        public abstract Task PublishAsync<T>(T message) ;
        public abstract Task PublishAsync(object message);
        public abstract void Publish(object message);


        public abstract void Publish<T>(IBroadcastEvent<T> evnt);
        public abstract Task PublishAsync<T>(IBroadcastEvent<T> evnt);
        public abstract Task PublishAsync(IBroadcastEvent evnt);
        public abstract void Publish(IBroadcastEvent evnt);

        public abstract List<ISubscription> Unsubscribe<T>();
        public void Unsubscribe(ISubscription subscription) => Unsubscribe(subscription.EventKey);
        public abstract void Unsubscribe(string eventKey);

        public static Broadcast Instance
        {
            get
            {
                return _Instance??new DefaultBroadcast();
            }
        }

        public static void Register(Broadcast broadcast)
        {
            _Instance = broadcast;
        }
    }
}
