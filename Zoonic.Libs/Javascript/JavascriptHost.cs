using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public sealed class JavascriptHost
    {
        private readonly JavaScriptNativeFunction SetTimeoutJavaScriptNativeFunction; // Required to keep a reference
        private readonly JavaScriptNativeFunction SendToHostJavaScriptNativeFunction; // Required to keep a reference
        private JavaScriptSourceContext currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);
        private readonly JavaScriptRuntime runtime;
        private JavaScriptValue promiseCallback;
        private readonly JavaScriptContext context;

        public JavascriptHost()
        {
            if (Native.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null, out runtime) !=
                JavaScriptErrorCode.NoError)
            {
                throw new Exception("failed to create runtime.");
            }

            if (Native.JsCreateContext(runtime, out context) != JavaScriptErrorCode.NoError)
                throw new Exception("failed to create execution context.");

            if (Native.JsSetCurrentContext(context) != JavaScriptErrorCode.NoError)
                throw new Exception("failed to set current context.");

            // ES6 Promise callback
            JavaScriptPromiseContinuationCallback promiseContinuationCallback =
                delegate (JavaScriptValue task, IntPtr callbackState)
                {
                    promiseCallback = task;
                };

            if (Native.JsSetPromiseContinuationCallback(promiseContinuationCallback, IntPtr.Zero) !=
                JavaScriptErrorCode.NoError)
                throw new Exception("failed to setup callback for ES6 Promise");

            // Bind to global object
            // setTimeout
            SetTimeoutJavaScriptNativeFunction = SetTimeout.SetTimeoutJavaScriptNativeFunction;
            DefineHostCallback("setTimeout", SetTimeoutJavaScriptNativeFunction);

            SendToHostJavaScriptNativeFunction = CommunicationManager.SendToHostJavaScriptNativeFunction;
            DefineHostCallback("sendToHost", SendToHostJavaScriptNativeFunction);

           
            // Debug
            if (Native.JsStartDebugging() != JavaScriptErrorCode.NoError)
                throw new Exception("failed to start debugging.");
        }
        
        public void ProjectNamespace(string namespaceName)
        {
            if (Native.JsProjectWinRTNamespace(namespaceName) != JavaScriptErrorCode.NoError)
                throw new Exception($"failed to project {namespaceName} namespace.");
        }

        public string RunScript(string script)
        {
            IntPtr returnValue;

            JavaScriptValue result;

            if (Native.JsRunScript(script, currentSourceContext++, "", out result) != JavaScriptErrorCode.NoError)
            {
                // Get error message and clear exception
                JavaScriptValue exception;
                if (Native.JsGetAndClearException(out exception) != JavaScriptErrorCode.NoError)
                    throw new Exception("failed to get and clear exception");

                JavaScriptPropertyId messageName;
                if (Native.JsGetPropertyIdFromName("message",
                    out messageName) != JavaScriptErrorCode.NoError)
                    throw new Exception("failed to get error message id");

                JavaScriptValue messageValue;
                if (Native.JsGetProperty(exception, messageName, out messageValue)
                    != JavaScriptErrorCode.NoError)
                    throw new Exception("failed to get error message");

                IntPtr message;
                UIntPtr length;
                if (Native.JsStringToPointer(messageValue, out message, out length) != JavaScriptErrorCode.NoError)
                    throw new Exception("failed to convert error message");

                return Marshal.PtrToStringUni(message);
            }

            // Execute promise tasks stored in promiseCallback 
            while (promiseCallback.IsValid)
            {
                JavaScriptValue task = promiseCallback;
                promiseCallback = JavaScriptValue.Invalid;
                JavaScriptValue promiseResult;
                Native.JsCallFunction(task, null, 0, out promiseResult);
            }

            // Convert the return value.s
            JavaScriptValue stringResult;
            UIntPtr stringLength;
            if (Native.JsConvertValueToString(result, out stringResult) != JavaScriptErrorCode.NoError)
                throw new Exception("failed to convert value to string.");
            if (Native.JsStringToPointer(stringResult, out returnValue, out stringLength) !=
                JavaScriptErrorCode.NoError)
                throw new Exception("failed to convert return value.");

            return Marshal.PtrToStringUni(returnValue);
        }

        public string ProjectObjectToGlobal(object objectToProject, string name)
        {
            JavaScriptValue value;
            if (Native.JsInspectableToObject(objectToProject, out value) != JavaScriptErrorCode.NoError)
                return $"failed to project {name} object";

            DefineHostProperty(name, value);

            return "NoError";
        }

        public void CallFunction(string name, params object[] parameters)
        {
            JavaScriptValue globalObject;
            Native.JsGetGlobalObject(out globalObject);

            JavaScriptPropertyId functionId = JavaScriptPropertyId.FromString(name);

            var function = globalObject.GetProperty(functionId);

            // Parameters
            var javascriptParameters = new List<JavaScriptValue>();

            javascriptParameters.Add(globalObject); // this value

            foreach (var parameter in parameters)
            {
                javascriptParameters.Add(JavaScriptValue.From(parameter));
                //var parameterType = parameter.GetType().Name;
                //switch (parameterType)
                //{
                //    case "Int32":
                //        javascriptParameters.Add(JavaScriptValue.FromInt32((int)parameter));
                //        break;
                //    case "Double":
                //        javascriptParameters.Add(JavaScriptValue.FromDouble((double)parameter));
                //        break;
                //    case "Boolean":
                //        javascriptParameters.Add(JavaScriptValue.FromBoolean((bool)parameter));
                //        break;
                //    case "String":
                //        javascriptParameters.Add(JavaScriptValue.FromString((string)parameter));
                //        break;
                //    default:
                //        throw new Exception("Not supported type: " + parameterType);
                //}
            }

            // call function
            function.CallFunction(javascriptParameters.ToArray());
        }

        public static void DefineHostCallback(JavaScriptValue globalObject, string callbackName, JavaScriptNativeFunction callback, IntPtr callbackData)
        {
            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);

            JavaScriptValue function = JavaScriptValue.CreateFunction(callback, callbackData);

            globalObject.SetProperty(propertyId, function, true);
        }
        // Private tools
        public static void DefineHostCallback(string callbackName, JavaScriptNativeFunction callback)
        {
            JavaScriptValue globalObject;
            Native.JsGetGlobalObject(out globalObject);

            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);
            JavaScriptValue function = JavaScriptValue.CreateFunction(callback, IntPtr.Zero);

            globalObject.SetProperty(propertyId, function, true);

            uint refCount;
            Native.JsAddRef(function, out refCount);
        }

        public static void DefineHostProperty(string callbackName, JavaScriptValue value)
        {
            JavaScriptValue globalObject;
            Native.JsGetGlobalObject(out globalObject);

            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);
            globalObject.SetProperty(propertyId, value, true);

            uint refCount;
            Native.JsAddRef(value, out refCount);
        }
        public static void DefineHostProperty(JavaScriptValue jsObject,string callbackName, JavaScriptValue value)
        {
            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);
            jsObject.SetProperty(propertyId, value, true);

            uint refCount;
            Native.JsAddRef(value, out refCount);
        }
    }
}
