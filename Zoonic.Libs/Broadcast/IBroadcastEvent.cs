using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Broadcast
{
    public interface IBroadcastEvent
    {
        object GetContent();
        TContent GetContent<TContent>();
    }
    public interface IBroadcastEvent<TContent>:IBroadcastEvent 
    {
        /// <summary>
        /// Gets the content of the message.
        /// </summary>
        TContent Content { get; }

    }
}
