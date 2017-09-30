using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Messages
{
    public struct UserState
    {
        public UserStateFlag Flag { get; set; }
        public string Position { get; set; }
        public double? ExpireMinutes { get; set; }
    }
    public enum UserStateFlag
    {

        /// <summary>
        /// 无状态
        /// </summary>
        None = 1,
        /// <summary>
        /// 已进入应用状态
        /// </summary>
        Enter = 2,
        /// <summary>
        /// 退出App状态（临时传输状态，退出后即为None）
        /// </summary>
        Exit = 4
    }
}
