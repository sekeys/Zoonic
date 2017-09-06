

namespace Zoonic.Web.Route
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    public class RegexConstraint : IConstraint
    {
        private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(10);
        public Regex Constraint { get; private set; }

        public string Segments => _routeKey;
        

        public string _routeKey;
        public RegexConstraint(string routekey,string regexPattern)
        {
            if (regexPattern == null)
            {
                throw new ArgumentNullException(nameof(regexPattern));
            }
            _routeKey = routekey;
            Constraint = new Regex(
                regexPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                RegexMatchTimeout);
        }
        public bool Match(string urlValue)
        {
            if (Constraint.IsMatch(urlValue))
            {
                return true;
            }
            return false;
        }
    }
}
//{value}
