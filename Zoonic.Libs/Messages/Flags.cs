using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Messages
{
    public class Flags
    {
        public const int TEXT = 1;
        public const int IMAGE = 2;
        public const int LOCALTION = 3;
        public const int EVENT = 4;
        public const int VOICE = 5;
        public const int LINK = 6;
        public const int SHORT_VIDEO = 7;
        public const int VIDEO = 8;
        public const int TEXT_COMMOMD = 30;
        public const int TEXT_WEATHER = 31;

        public static Zoonic.Collection.ImmutableDictionary<string, int> Flag
        {
            get;
            private set;
        }
        static Flags()
        {
            if (Flag == null)
            {
                Flag = Zoonic.Collection.ImmutableDictionary<string, int>.Empty;
            }
        }
        public static int Type(string key)
        {
            switch (key.ToUpper())
            {
                case "TEXT": return MessageType.TEXT;
                case "IMAGE": return MessageType.IMAGE;
                case "LOCALTION": return MessageType.LOCALTION;
                case "EVENT": return MessageType.EVENT;
                case "VOICE": return MessageType.VOICE;
                case "LINK": return MessageType.LINK;
                case "SHORT_VIDEO": return MessageType.SHORT_VIDEO;
                case "VIDEO": return MessageType.VIDEO;
                case "TEXT_COMMOMD": return MessageType.TEXT_COMMOMD;
                case "TEXT_WEATHER": return MessageType.TEXT_WEATHER;
                default:
                    return Flag[key];
            }
        }
    }
}
