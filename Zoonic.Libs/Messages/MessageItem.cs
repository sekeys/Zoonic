using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Authentication;

namespace Zoonic.Messages
{
    public class MessageItem
    {
        public IUser User { get; set; }
        public UserState State { get; set; }
        public IRequestMessageBase Request { get; set; }
        public IResponseMessageBase Response { get; set; }

        public Task Initialize(MessageContext context)
        {
            return null; 
        }
        public Task<bool> Distinct()
        {
            return Task.FromResult(false);
        }
    }
}
