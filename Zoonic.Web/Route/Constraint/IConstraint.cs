using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Route
{
    public interface IConstraint
    {
        string Segments { get;  }
        bool Match(string urlValue);
    }
}

//  {value}