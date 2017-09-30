

namespace Zoonic.Messages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Zoonic;
    public abstract class MessageHandler : IRunnable
    {
        public MessageContext MessageContext { get; }
        public IRequestMessageBase RequestMessage { get; protected set; }
        public IResponseMessageBase ResponseMessage { get; protected set; }
        public  MessageItem MessageItem { get; protected set; }

        public MessageHandler()
        {

        }
        public abstract void Initialize(Stream stream);
        public abstract void Run();
        public abstract void OnEvent(IRequestMessageBase message);
        public abstract void OnRunning();
        public abstract void OnRunned();
        protected bool Cancel { get; set; }
    }
}
