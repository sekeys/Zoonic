

namespace Zoonic.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Zoonic;

    public class VerbAttribute:Attribute
    {
        public Verbs Verb { get; private set; }
        public VerbAttribute(Verbs verb)
        {
            Verb = verb;
        }
    }
}
