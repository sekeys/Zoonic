using System;
using System.Xml.Linq;

namespace Zoonic.Messages
{
    /// <summary>
    /// 文本类型消息
    /// </summary>
    public class RequestMessageText : RequestMessageBase, IRequestMessageBase
    {
        public RequestMessageText(XDocument doc)
        {
            Initialize(doc);
        }

        protected void Initialize(XDocument doc)
        {
            this.ToUserName = doc.Root.Element(Core.Constants.TOUSERNAME)?.Value;
            this.FromUserName = doc.Root.Element(Core.Constants.FROMUSERNAME)?.Value;
            this.CreateTime = Convert.ToInt64( doc.Root.Element(Core.Constants.CREATETIME)?.Value);
            //this.MsgType = MsgTypeHelper.GetRequestMsgType(doc);
            this.Content = doc.Root.Element(Core.Constants.CONTENT)?.Value;
            this.MsgId = Convert.ToInt64(doc.Root.Element(Core.Constants.MSGID)?.Value);
        }
        public override RequestMsgType MsgType
        {
            get { return RequestMsgType.Text; }
        }

        /// <summary>
        /// 文本消息内容
        /// </summary>
        public string Content { get; set; }
    }
}
