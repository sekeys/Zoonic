using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zoonic.Messages
{
    public class MessagingHandler : MessageHandler
    {

        public override void Initialize(Stream stream)
        {
            this.RequestMessage = RequestMessageFactory.Factory.GetRequest(stream);
        }

        public override void OnEvent(IRequestMessageBase message)
        {
            
        }

        public override void OnRunned()
        {
        }

        public override void OnRunning()
        {
        }

        public override async void Run()
        {
            this.OnRunning();
            if (this.MessageItem != null)
            {
                await this.MessageItem.Initialize(this.MessageContext);

                if (await this.MessageItem.Distinct())
                {
                    Cancel = true;
                }
            }
            if (!Cancel)
            {
                OnEvent(RequestMessage);
            }
            this.OnRunned();
        }
    }
}
