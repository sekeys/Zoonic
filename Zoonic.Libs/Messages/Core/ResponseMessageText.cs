using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Messages
{
    public class ResponseMessageText:IResponseMessageBase
    {

        public new virtual ResponseMsgType MsgType
        {
            get { return ResponseMsgType.Text; }
        }

        public string Content { get; set; }
        public string ToUserName { get;  set; }
        public string FromUserName { get;  set; }
        public long CreateTime { get;  set; }
    }
}
