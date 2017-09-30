using System;
using System.Collections.Generic;
using System.Text;
using Zoonic;
using Zoonic.Authentication;

namespace Zoonic.Messages
{

    public abstract class MessageContext
    {
        public IUser User { get; private set; }
        //public abstract void Push(T msg);
        //public abstract T Pop();
        //public abstract T[] Pop(int count);
        //public abstract T[] PopAll();
        public abstract void Enter(MessageItem request);
        public Double? ExpireMinutes { get; set; }

        public UserState State { get; set; }
        public dynamic StorageData { get; private set; } = new IgnoreDynamic();
        public abstract bool Distinct(IMessageBase request);

        /// <summary>
        /// 最大储存容量（分别针对RequestMessages和ResponseMessages）
        /// </summary>
        public int MaxRecordCount { get; set; }

        public static Func<MessageContext> HowToFind = null;

        public static MessageContext Context
        {
            get
            {
                if (HowToFind != null)
                {
                    return HowToFind();
                }
                return null;
            }
        }
    }
}
