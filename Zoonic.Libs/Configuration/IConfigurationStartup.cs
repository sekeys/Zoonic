using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoonic.Configuration
{
    public interface IConfigurationStartup: IConfigurationStartupBase
    {
        IConfigurationStartup Parent { get; set; }
        void BuildConfigureStartup();
        string UnionCode { get; }
        ConfigurationManager Manager { get; }
        IConfiguration Section { get; set; }
        int Priority { get; set; }
        IConfigurationStartup Original { get; set; }
    }
}
