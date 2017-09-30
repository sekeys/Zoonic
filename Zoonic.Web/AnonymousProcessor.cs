using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Attributes;

namespace Zoonic.Web
{
    public class AnonymousProcessor:ProcessorBase
    {
        public string Text { get; private set; }

        public bool AllowDebug { get; private set; }
        public Type Type { get; private set; }
        
        public override async Task<ProcessorResult> Process()
        {
            return await Task.Run<ProcessorResult>(() =>
            {
                var ti = Type.GetTypeInfo();
                var authes = ti.GetCustomAttributes(typeof(IAuthenticationAttribute)) as IEnumerable<IAuthenticationAttribute>;
                foreach (var item in authes)
                {
                    if (item is IAuthenticationAttribute)
                    {
                        ((IAuthenticationAttribute)(item)).Authorized(this.User);
                    }
                }
                object obj = Activator.CreateInstance(Type);

                var result=ti.GetMethod("Invoke").Invoke(obj, null);
                if(result is ProcessorResult)
                {
                    return (ProcessorResult)result;
                }
                else if(result is Task<ProcessorResult>)
                {
                    var t = (Task<ProcessorResult>)result;
                    t.Wait();
                    return t.Result;
                }
                else if(result is Task<Object>)
                {

                    var t = (Task<ProcessorResult>)result;
                    t.Wait();
                    return CreateResult(t.Result);
                }
                else
                {
                    return CreateResult(result);
                }
                return null;
            });
        }
        private ProcessorResult CreateResult(object data)
        {
            if(data is Exception)
            {
                return new StatusResult((Exception)data);
            }
            else
            {
                return new JsonResult(data);
            }
        }

        public AnonymousProcessor(object obj)
        {
            Type = obj.GetType();
            string name = Type.Name;
            name = name.ToLower();
            if (name.EndsWith("Processor", StringComparison.OrdinalIgnoreCase))
            {
                Text = name.Substring(0, name.Length - 9);
            }

        }
    }
}
