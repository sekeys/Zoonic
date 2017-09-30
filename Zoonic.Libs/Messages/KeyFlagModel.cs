using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Messages
{
    using Zoonic.Collection;
    public class StringContent
    {
        public string Content { get; set; }
        public bool Interrogative { get; set; } = false;
         
        public bool Location
        {
            get { return false; }
        }
        public bool Weather
        {
            get;
        }
    }
    public class KeyFlagModel
    {
        public string Word { get; set; }
        public string Features { get; set; }
        public ImmutableList<string> Flag { get; set; }
        public bool Interrogative { get; set; } = false;
        public bool Location
        {
            get
            {
                return Flag.Contains("location");//"location".Equals(Flag,StringComparison.OrdinalIgnoreCase);
            }
        }
        public bool Unknow
        {
            get
            {
                return Flag.Contains("unknown"); ;
            }
        }
    }
}
