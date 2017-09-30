using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic
{
    public interface IClonable<T>
    {
        T Clone();
    }
}
