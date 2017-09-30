using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Messages
{
    public abstract class ResponseMessageFactory
    {
        private static ResponseMessageFactory _Factory;
        public abstract IResponseMessageBase Response(IRequestMessageBase requestMessage);
        public static ResponseMessageFactory Factory
        {
            get
            {
                return _Factory;
            }
        }
        public static void Register(ResponseMessageFactory factory)
        {
            _Factory = factory;
        }
    }
}
