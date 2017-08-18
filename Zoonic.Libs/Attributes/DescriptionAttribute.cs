using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Attributes
{
    public class DescriptionAttribute:Attribute
    {
        public string Description { get;private set; }
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

    }
}
