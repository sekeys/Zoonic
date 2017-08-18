using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Lib;

namespace Zoonic.Interface
{
    public interface IPriorityRunnable:IRunnable
    {
        int Priority { get; }
    }
    public interface IPriorityRunnable<T> : IRunnable<T>
    {
        int Priority { get; }
    }
    public interface IPriorityRunnable<T,T2> : IRunnable<T, T2>
    {
        int Priority { get; }
    }
}
