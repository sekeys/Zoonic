using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Attributes;
using Zoonic.Web.Exceptions;

namespace Zoonic.Web
{
    public class RestfulProcessorBase : ProcessorBase
    {
        public string Method => Context.Request.Method;
        public static Type TaskType = typeof(Task);
        public RestfulProcessorBase()
        {
        }
        public override Task<ProcessorResult> Process()
        {
            //sReequest

            return Task<ProcessorResult>.Run(() =>
           {
               object returnObj = null;
               try
               {
                   var rest = this.GetType().GetMethod(Method, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
                   if (rest == null)
                   {
                       throw new StatusException("未找相关的实现方法", 404);
                   }
                   var attributes = rest.GetCustomAttributes().Where(m => m is Zoonic.Attributes.IAuthenticationAttribute) as IEnumerable<IAuthenticationAttribute>;
                   if (attributes != null)
                   {
                       foreach (var attribute in attributes)
                       {
                           attribute.Authorized(User);
                       }
                   }
                   if (rest != null)
                   {
                       if (rest.ReturnType.BaseType == TaskType || rest.ReturnType == TaskType)
                       {
                           if (rest.ReturnType.IsGenericType)
                           {
                               var task1 = rest.Invoke(this, null);
                               var task = task1 as Task;
                               if (task != null)
                               {
                                   task.Wait();
                                   var pro = task.GetType().GetProperty("Result");
                                   returnObj = pro?.GetValue(task);//await task.Unwrap<object>();
                               }
                           }
                           else
                           {
                               var task = rest.Invoke(this, null) as Task;
                               if (task != null)
                               {
                                   task.Wait();
                               }
                           }
                       }
                       else
                       {
                           returnObj = rest.Invoke(this, null);
                       }
                   }
                   if (returnObj == null)
                   {
                       return new ContentResult("");
                   }
                   else if (returnObj is ProcessorResult)
                   {
                       return returnObj as ProcessorResult;
                   }
                   else if (returnObj is String)
                   {
                       return new ContentResult(returnObj.ToString());
                   }
                   else
                   {
                       return new JsonResult(returnObj);
                   }
               }
               catch (Exception ex)
               {
                   throw new StatusException("未找相关的实现方法", 500);
               }
           });
        }
    }
}
