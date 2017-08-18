using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib
{
    public interface IClonable<T>
    {
        T Clone();
    }
}
