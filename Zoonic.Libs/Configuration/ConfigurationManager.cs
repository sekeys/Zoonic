using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoonic.Configuration
{
    public class ConfigurationManager
    {
        public Dictionary<string, IConfigurationStartup> Startups { get; private set; }
        public ConfigurationManager()
        {
            Startups = new Dictionary<string, IConfigurationStartup>();
        }

        public IEnumerable<IConfigurationSection> Sections(string key)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups)
            {
                sections.Add(sc.Value.Section.GetSection(key));
            }
            return sections;
        }
        public IEnumerable<IConfigurationSection> Sections(string key, Func<IConfiguration, IConfigurationSection> filter)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups)
            {
                sections.Add(filter.Invoke(sc.Value.Section));
            }
            return sections;
        }
        public IEnumerable<IConfigurationSection> Sections(string key, Func<IConfiguration, IEnumerable<IConfigurationSection>> filter)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups)
            {
                sections.AddRange(filter.Invoke(sc.Value.Section));
            }
            return sections;
        }
        public void Action(Action<IEnumerable<IConfigurationSection>> action, string key)
        {
            action.Invoke(Sections(key));
        }
        public Task ActionAsync(Action<IEnumerable<IConfigurationSection>> action, string key)
        {
            return Task.Run(() => { Action(action, key); });
        }

        public void Action(Action<IEnumerable<IConfiguration>> action, string key)
        {
            action.Invoke(Sections(key));
        }
        public Task ActionAsync(Action<IEnumerable<IConfiguration>> action, string key)
        {
            return Task.Run(() => { Action(action, key); });
        }


        public IEnumerable<IConfigurationSection> Sections(string key, string unioncode)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups.Where(m => m.Value.UnionCode.Equals(unioncode, StringComparison.OrdinalIgnoreCase)))
            {
                sections.Add(sc.Value.Section.GetSection(key));
            }
            return sections;
        }
        public IEnumerable<IConfigurationSection> Sections(string key, Func<IConfiguration, IConfigurationSection> filter, string unioncode)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups.Where(m => m.Value.UnionCode.Equals(unioncode, StringComparison.OrdinalIgnoreCase)))
            {
                sections.Add(filter.Invoke(sc.Value.Section));
            }
            return sections;
        }
        public IEnumerable<IConfigurationSection> Sections(string key, Func<IConfiguration, IEnumerable<IConfigurationSection>> filter, string unioncode)
        {
            List<IConfigurationSection> sections = new List<IConfigurationSection>();
            foreach (var sc in Startups.Where(m => m.Value.UnionCode.Equals(unioncode, StringComparison.OrdinalIgnoreCase)))
            {
                sections.AddRange(filter.Invoke(sc.Value.Section));
            }
            return sections;
        }
        public void Action(Action<IEnumerable<IConfigurationSection>> action, string key, string unioncode)
        {
            action.Invoke(Sections(key, unioncode));
        }
        public Task ActionAsync(Action<IEnumerable<IConfigurationSection>> action, string key, string unioncode)
        {
            return Task.Run(() => { Action(action, key, unioncode); });
        }

        public void Action(Action<IEnumerable<IConfiguration>> action, string key, string unioncode)
        {
            action.Invoke(Sections(key));
        }
        public Task ActionAsync(Action<IEnumerable<IConfiguration>> action, string key, string unioncode)
        {
            return Task.Run(() => { Action(action, key, unioncode); });
        }

        public void Configure()
        {
            foreach (var configure in Startups.Values.OrderBy(m => m.Priority))
            {
                configure.Configure();
            }
        }
        public ConfigurationManager AddFile(string path,  int priority = 99)
        {
            ConfigurationStartup cs = new ConfigurationStartup(path);
            cs.Priority = priority;
            //this.Startups.Add(cs.UnionCode, cs);
            return this;

        }
        private static ConfigurationManager manager;
        public static ConfigurationManager Manager
        {
            get
            {
                if (manager == null)
                {
                    manager = new ConfigurationManager();
                }
                return manager;
            }
        }
    }
}
