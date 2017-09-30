using System;

namespace Zoonic.Messages
{
    public interface IEntityBase
    {

    }
    /// <summary>
    /// 所有Request和Response消息的接口
    /// </summary>
    public interface IMessageBase : IEntityBase
    {
        /// <summary>
        /// 接收人（在 Request 中为公众号的微信号，在 Response 中为 OpenId）
        /// </summary>
        string ToUserName { get;  }
        /// <summary>
        /// 发送人（在 Request 中为OpenId，在 Response 中为公众号的微信号）
        /// </summary>
        string FromUserName { get;  }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        long CreateTime { get;  }
    }
    public interface IRequestMessageBase: IMessageBase
    {
        RequestMsgType MsgType { get; }
        string Encrypt { get; set; }
        long MsgId { get; set; }
    }

    /// <summary>
    /// 接收到请求的消息基类
    /// </summary>
    public class RequestMessageBase : IRequestMessageBase
    {
        public RequestMessageBase()
        {

        }
        public long MsgId { get; set; }
        public virtual RequestMsgType MsgType
        {
            get { return RequestMsgType.Text; }
        }

        public string Encrypt { get; set; }
        public string ToUserName { get ; protected set; }
        public string FromUserName { get; protected set; }
        public long CreateTime { get; protected set; }
    }
}
