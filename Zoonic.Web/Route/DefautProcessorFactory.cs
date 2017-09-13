using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Route
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using Zoonic.Lib;

    public abstract class ProcessorFactory
    {

        public abstract IProcessor Cache(string text);
        public abstract void Cache(string key,IProcessor middle);

        public abstract void Cache<T>(string key);
        public abstract void Cache(string key,Type type);

        public static ProcessorFactory Factory
        {
            get;
            private set;
        }
        public static void Register(ProcessorFactory factory)
        {
            Factory = factory;
        }

        public abstract IEnumerable<KeyValuePair<string, IProcessor>> Caches();
    }

    public class DefaultProcessorFactory : ProcessorFactory
    {
        protected ConcurrentDictionary<string, IProcessor> Middles = new ConcurrentDictionary<string, IProcessor>();
        public override IEnumerable<KeyValuePair<string, IProcessor>> Caches()
        {
            return Middles.ToArray();
        }
        public override IProcessor Cache(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { return null; }
            text =  text.ToLower();
            var ctx = AccessorContext.DefaultContext.Get<HttpContext>();
            if (Middles.ContainsKey(text))
            {
                return Middles[text];
            }
            text = text + "_" + ctx.Request.Method.ToUpper();
            if (ctx != null && Middles.ContainsKey(text))
            {
                return Middles[text];
            }
            return null;
        }

        public override void Cache(string key,IProcessor middle)
        {
            Middles.TryAdd(key.ToLower(), middle);
        }

        public override void Cache<T>(string key)
        {
            var type = typeof(T);
            if (type.GetTypeInfo().IsAssignableFrom(typeof(IProcessor)))
            {
                Cache(key,Activator.CreateInstance(type) as IProcessor);
            }
            else
            {
                Cache(key,new AnonymousProcessor(Activator.CreateInstance(type)));
            }
        }
        public override void Cache(string key,Type type)
        {
            if (typeof(IProcessor).GetTypeInfo().IsAssignableFrom(type))
            {
                Cache(key,Activator.CreateInstance(type) as IProcessor);
            }
            else
            {
                Cache(key,new AnonymousProcessor(Activator.CreateInstance(type)));
            }
        }

    }
}
