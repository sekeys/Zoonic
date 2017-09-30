using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic
{
    public interface IRunnable
    {
        void Run();
    }

    public interface IRunnable<T>
    {
        void Run(T parameter);
    }
    public interface IRunnable<in T,out T2>
    {
        T2 Run(T parameter);
    }
}
