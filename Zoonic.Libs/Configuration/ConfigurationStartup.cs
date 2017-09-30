
namespace Zoonic.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using System.Reflection;
    using System.Runtime.Loader;

    public class ConfigurationStartup : IConfigurationStartup
    {

        public int Priority { get; set; } = 0;
        public ConfigurationManager Manager { get { return ConfigurationManager.Manager; } }
        public static string RootConfigurePath { get; set; }
        protected string m_ConfigurePath;
        public string ConfigurePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_ConfigurePath)) { return RootConfigurePath; }
                return m_ConfigurePath;
            }
            set { m_ConfigurePath = value; }
        }
        public IConfiguration Section { get; set; }

        public string UnionCode
        {
            get;
            private set;
        }

        public IConfigurationStartup Parent
        {
            get;
            set;
        } = null;


        public IConfigurationStartup Original
        {
            get;
            set;
        } = null;

        public ConfigurationStartup()
        {
            var cb = new ConfigurationBuilder();
            Section =cb.AddJsonFile(ConfigurePath, true, true).Build();
            UnionCode = Section.GetSection("unioncode").Value;
            if (!string.IsNullOrWhiteSpace(UnionCode))
            {
                if (Manager.Startups.ContainsKey(this.UnionCode))
                {
                    Manager.Startups[this.UnionCode] = this;
                }
                else
                {
                    Manager.Startups.Add(this.UnionCode, this);
                }
                BuildConfigureStartup();
            }

        }

        public ConfigurationStartup(string configpath)
        {
            m_ConfigurePath = configpath;
            var cb = new ConfigurationBuilder();
            Section = cb.AddJsonFile(ConfigurePath, true, true).Build();
            UnionCode = Section.GetSection("unioncode").Value;
            Manager.Startups.Add(this.UnionCode, this);
            if (Manager.Startups.ContainsKey(this.UnionCode))
            {
                Manager.Startups[this.UnionCode] = this;
            }
            else
            {
                Manager.Startups.Add(this.UnionCode, this);
            }
            BuildConfigureStartup();

        }
        /// <summary>
        /// Build Configure Starup
        /// </summary>
        public void BuildConfigureStartup()
        {
            IConfigurationSection startupSection = Section.GetSection("startups");
            object obj = null;
            IConfigurationStartup cs = null;
            foreach (var section in startupSection.GetChildren())
            {
                var assemlySection = section.GetSection("assemly");
                var typeSection = section.GetSection("type");
                var pathSection = section.GetSection("path");
                if (assemlySection == null && assemlySection.Value == null && pathSection == null)
                {
                    continue;
                }
                else if ((assemlySection == null || assemlySection.Value == null) && pathSection != null && pathSection.Value != null)
                {
                    cs = new ConfigurationStartup(pathSection.Value);

                }
                else
                {
                    var ass = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemlySection.Value);
                    var type = ass.GetType(typeSection.Value);
                    obj = System.Activator.CreateInstance(type);
                }
                if (obj is IConfigurationStartup)
                {
                    cs = obj as IConfigurationStartup;
                }
                else if (obj is IConfigurationSectionStartup)
                {
                    cs = obj as IConfigurationSectionStartup;
                    cs.Parent = this;
                    cs.Section = Section.GetSection(section.GetSection("section").Value);
                }
                var prioritySection = section.GetSection("priority");
                if (prioritySection == null)
                {
                    cs.Priority = 1000;
                }
                else
                {
                    cs.Priority = Convert.ToInt32(prioritySection.Value);
                }
                cs.Original = this;
                //cs.BuildConfigureStartup();
                //if (Manager.Startups.ContainsKey(cs.UnionCode))
                //{
                //    Manager.Startups[cs.UnionCode] = cs;
                //}
                //else
                //{
                //    Manager.Startups.Add(cs.UnionCode, cs);
                //}
            }
        }

        public void Configure()
        {
            BuildAppsetting();
            //BuildMiddleware();
        }
        //public void BuildMiddleware()
        //{
        //    var middles = Section.GetSection("Middlewares");
        //    Type type = null;
        //    foreach (ConfigurationSection cs in middles.GetChildren())
        //    {
        //        var values = cs.Value.Split(';');
        //        string assemlyDll = values[0].Replace("~", Appsetting.Appsettings["hostdir"].ToString());

        //        var assemly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemlyDll);
        //        if (assemly != null)
        //        {
        //            type = assemly.GetType(values[1]);
        //            Zoonic.W.Factory.Cache(type);
        //        }
        //    }
        //}
        /// <summary>
        /// 初始化Appsetting配置
        /// </summary>
        public void BuildAppsetting()
        {
            var aps = Section.GetSection("appsettings");
            foreach (ConfigurationSection cs in aps.GetChildren())
            {
                if ("viewrootpath".Equals(cs.Key, StringComparison.OrdinalIgnoreCase))
                {
                    Appsetting.AppSettings[cs.Key] = cs.Value.Replace("~", Appsetting.AppSettings.Get<string>("hostdir"));
                }
                else
                {
                    Appsetting.AppSettings[cs.Key] = cs.Value;
                }
            }
        }


    }
}
