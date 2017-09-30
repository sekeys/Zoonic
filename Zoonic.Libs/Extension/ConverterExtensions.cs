using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Zoonic.Extension.Converter
{
    public static class ConverterExtensions
    {
        public static XDocument ToXDoc(this IXmlConverter converter)
        {
            var xml = XDocument.Parse(converter.ToXml());
            return xml;
        }
        public static JObject ToJObject(this IJsonConverter converter)
        {
            return JObject.Parse(converter.ToJson());

        }
        public static XDocument ToXDoc(this string xmlDoc)
        {
            var xml = XDocument.Parse(xmlDoc);
            return xml;
        }
        public static JObject ToJObject(this string json)
        {
            return JObject.Parse(json);

        }
    }
}
