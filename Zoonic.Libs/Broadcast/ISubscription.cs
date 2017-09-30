using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Broadcast
{
    using System;
    using Zoonic;

    /// <summary>
    /// Provides a contract to Types wanting to subscribe to published messages 
    /// with conditions and a callback.
    /// </summary>
    public interface ISubscription : IRunnable<IBroadcastEvent>
    {
        string EventKey { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ISubscription"/> is active.
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Provides a contract to Types wanting to subscribe to published messages 
    /// with conditions and a callback.
    /// </summary>
    public interface ISubscription<T> :ISubscription, IRunnable<IBroadcastEvent<T>>
    {

    }
}
