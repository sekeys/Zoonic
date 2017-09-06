using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Route
{
    public class StringConstraint: IConstraint
    {
        private string _value;
        public string _routeKey;

        public string Segments => _routeKey;

        public StringConstraint(string value)
        {
            _value = value;
            _routeKey = value;
        }
        

        public bool Match(string urlValue)
        {
            return _value.Equals(urlValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
