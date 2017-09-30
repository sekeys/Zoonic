using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Javascript.Hosting;
using Zoonic.Javascript.Interface;

namespace Zoonic.Javascript
{
    public class Asyncable : IAsyncable
    {
        public JavaScriptValue Data { get; set; }
        public Task<JavaScriptValue> Task { get; protected set; }
        public bool IsCompleted => Task.IsCompleted;
        protected Asyncable()
        {
            DelegateWait = JavascriptWait;
            DelegateCatch = JavascriptCatch;
            DelegateResult = JavascriptResult;
            DelegateThen = JavascriptThen;

        }
        public Asyncable(Task<JavaScriptValue> task):this()
        {
            Task = task;
            Task.ContinueWith(m => TaskContinueProcess(Task));
        }

        protected virtual void TaskContinueProcess(Task<JavaScriptValue> task)
        {
            Data = task.Result;
        }

        public void Catch(JavaScriptValue callback)
        {
            if (Task.IsFaulted || Task.Exception != null)
            {
                var ex = JavaScriptValue.CreateObject();
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("message"), JavaScriptValue.FromString(Task.Exception.Message), true);
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("stackTrace"), JavaScriptValue.FromString(Task.Exception.StackTrace), true);
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("source"), JavaScriptValue.FromString(Task.Exception.Source), true);
                callback.CallFunction(ex);
            }
        }

        public IAsyncableResult Result()
        {
            return new AsyncableResult(this);
        }

        public void Then(JavaScriptValue callback)
        {
            if (Task.IsCompleted)
            {
                callback.CallFunction(Task.Result);
            }
            else
            {
                Task.ContinueWith<JavaScriptValue>((t) =>
                {
                    var js = t.Result;
                    return callback.CallFunction(js);
                });
            }
        }

        public JavaScriptValue Wait(JavaScriptValue timeout)
        {
            
            JavaScriptValue javaScript = JavaScriptValue.CreateObject();
            if (!Task.IsCompleted)
            {
                if (timeout.ValueType == JavaScriptValueType.Number)
                {
                    Task.Wait(timeout.ConvertToNumber().ToInt32());
                }
                else if(timeout.ValueType== JavaScriptValueType.Function)
                {
                    this.Then(timeout);
                    this.Task.Wait();
                }
                else
                {
                    Task.Wait();
                }
            }
            if (Task.IsFaulted)
            {
                javaScript.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32(100), true);
                var ex = JavaScriptValue.CreateObject();
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("message"), JavaScriptValue.FromString(Task.Exception.Message), true);
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("stackTrace"), JavaScriptValue.FromString(Task.Exception.StackTrace), true);
                ex.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("source"), JavaScriptValue.FromString(Task.Exception.Source), true);
                javaScript.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("ex"), ex, true);
            }
            else
            {
                javaScript.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32(0), true);
                javaScript.SetProperty(Javascript.Hosting.JavaScriptPropertyId.FromString("status"), Task.Result, true);
            }
            return javaScript;
        }

        private JavaScriptNativeFunction DelegateWait;
        private JavaScriptNativeFunction DelegateThen;
        private JavaScriptNativeFunction DelegateCatch;
        private JavaScriptNativeFunction DelegateResult;

        protected virtual JavaScriptValue JavascriptWait(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            if (arguments.Length > 1)
            {
                var timeout = arguments[1];
                return this.Wait(arguments[1]);
            }
            return JavaScriptValue.Undefined;
        }
        protected virtual JavaScriptValue JavascriptThen(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var filter = arguments.Where(m => m.ValueType == JavaScriptValueType.Function);
            var length = filter.Count();
            for (var i = 1; i < length; i++)
            {
                this.Then(filter.ElementAt(i));
            }
            return arguments[0];
        }
        protected virtual JavaScriptValue JavascriptCatch(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var filter = arguments.Where(m => m.ValueType == JavaScriptValueType.Function);
            var length = filter.Count();
            for (var i = 1; i < length; i++)
            {
                this.Catch(filter.ElementAt(i));
            }
            return arguments[0];
        }
        protected virtual JavaScriptValue JavascriptResult(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            return JavaScriptValue.Undefined;//AsyncResult.Wrap(this.Result()); 
        }

        public static JavaScriptValue Register(Task<JavaScriptValue> asyncable)
        {
            var asyncObject = new Asyncable(asyncable);
            var jsValue = JavaScriptValue.CreateObject();
            JavascriptHost.DefineHostCallback(jsValue, "wait", asyncObject.DelegateWait, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "then", asyncObject.DelegateThen, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "catch", asyncObject.DelegateCatch, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "wait", asyncObject.DelegateResult, IntPtr.Zero);

            return jsValue;
        }

        public virtual JavaScriptValue Register()
        {
            var jsValue = JavaScriptValue.CreateObject();
            JavascriptHost.DefineHostCallback(jsValue, "wait", this.DelegateWait, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "then", this.DelegateThen, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "catch", this.DelegateCatch, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(jsValue, "wait", this.DelegateResult, IntPtr.Zero);

            return jsValue;
        }
    }
}
