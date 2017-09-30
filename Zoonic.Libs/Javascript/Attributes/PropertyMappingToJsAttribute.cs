using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Javascript.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field
        | AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyMappingToJsAttribute : Attribute
    {
        public string Property { get;private set; }
        public PropertyMappingToJsAttribute()
        {

        }
        public PropertyMappingToJsAttribute(string property)
        {
            Property = property;
        }
    }
}
