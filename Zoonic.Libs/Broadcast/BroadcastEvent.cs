using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Broadcast
{
    public class BroadcastEvent : IBroadcastEvent
    {
        protected object Content { get; set; }
        public object GetContent()
        {
            return Content;
        }

        public TContent GetContent<TContent>()
        {
            return Content is TContent ? (TContent)Content : default(TContent);
        }
        public  static BroadcastEvent Create(object content)
        {
            return new BroadcastEvent() { Content = content };
        }
    }
    public class BroadcastEvent<T> :BroadcastEvent, IBroadcastEvent<T>
    {

        public new T Content { get;protected set; }
        
        public static BroadcastEvent<T> Create(T content)
        {
            return new BroadcastEvent<T>() { Content = content };
        }
    }
}
