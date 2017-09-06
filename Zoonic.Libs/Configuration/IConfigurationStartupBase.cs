using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoonic.Configuration
{
    public interface IConfigurationStartupBase 
    {
        string ConfigurePath { get; }
        void Configure();

    }


}
