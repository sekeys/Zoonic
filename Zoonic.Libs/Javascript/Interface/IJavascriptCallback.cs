using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Javascript.Interface
{
    public interface IJavascriptCallback
    {
        void Call();
    }
    public interface IJavascriptCallback<T>
    {
        void Call(T param);
    }
    public interface IJavascriptCallback<T,T1>
    {
        void Call(T param,T1 param1);
    }
    public interface IJavascriptCallback<T,T1,T2>
    {
        void Call(T param, T1 param1, T2 param2);
    }
}
