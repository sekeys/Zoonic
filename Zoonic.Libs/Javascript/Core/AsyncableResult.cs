using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Javascript.Hosting;
using Zoonic.Javascript.Interface;

namespace Zoonic.Javascript
{
    public class AsyncableResult : Asyncable, IAsyncableResult
    {
        public AsyncableResult(Task<JavaScriptValue> task) : base(task)
        {
        }
        public AsyncableResult(Asyncable asyncable) : this(asyncable.Task)
        {
        }
        protected override void TaskContinueProcess(Task<JavaScriptValue> task)
        {
            base.TaskContinueProcess(task);
            var js = task.Result;
            var status = js.GetProperty(JavaScriptPropertyId.FromString("status"));
            if (status.IsValid && status.ValueType == JavaScriptValueType.Number)
            {
                Status = status.ConvertToNumber().ToInt32();
            }
            var message = js.GetProperty(JavaScriptPropertyId.FromString("message"));
            if (message.IsValid)
            {
                Message = message.ConvertToString().ToString();
            }
        }
        public int Status { get; private set; }

        public string Message { get; private set; }

        public override JavaScriptValue Register()
        {
            var jsValue = base.Register();
            
            return jsValue;
        }

        protected override JavaScriptValue JavascriptResult(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            return this.Data;
        }

        protected virtual JavaScriptValue JavascriptCompleted(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            return JavaScriptValue.Undefined;
        }
    }
}
