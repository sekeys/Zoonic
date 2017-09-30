using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Configuration
{
    public class MessageConfigurationStartup
    {
        public ConfigurationManager Manager { get; private set; }
        public MessageConfigurationStartup(ConfigurationManager configurationManager)
        {
            Manager = configurationManager;
        }
        public void Configure()
        {
            foreach(var kv in Manager.Startups)
            {
               if(kv.Value.Section != null)
                {
                    var messageFactory = kv.Value.Section.GetSection("factory");
                }
            }
        }
    }
}
