using Zoonic.Concurrency;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public interface IPipeline:IEnumerable<IHandler>
    {
        IHandlerContext Context(string name);
        IHandlerContext Context<T>();
        IPipeline AddFirst(string Name, IHandler value);

        IPipeline AddFirst(IExecutorGroup group, string name, IHandler value);

        IPipeline AddLast(string name, IHandler value);

        IPipeline AddLast(IExecutorGroup group, string name, IHandler value);


        IPipeline AddBefore(string beForename,string name, IHandler value);

        IPipeline AddLast(IExecutorGroup group,string beforeName, string name, IHandler value);


        IPipeline AddAfter(string beForename, string name, IHandler value);

        IPipeline AddAfter(IExecutorGroup group, string beforeName, string name, IHandler value);

        IPipeline AddFirst(params IHandler[] values);
        IPipeline AddFirst(IExecutorGroup group,params IHandler[] values);


        IPipeline AddLast(params IHandler[] values);
        IPipeline AddLast(IExecutorGroup group, params IHandler[] values);

        IPipeline Remove(IHandler value);
        IHandler Remove(string name);
        T1 Remove<T1>() where T1 : class, IHandler;
        IHandler RemoveFirst();
        IHandler RemoveLast();

        IPipeline Replace(IHandler old, string newName, IHandler newValue);
        IPipeline Replace(string old, string newName, IHandler newValue);

        IPipeline Replace<T1>(string newName, IHandler newValue) where T1 : class, IHandler;
        IHandler First();
        IHandler Last();

        IHandler Get(string name);
        T1 Get<T1>() where T1 : class;

        PipelineState State { get; }
        void Start();

        void Next(IHandlerContext context);
        void Cancel();
        void Completed();
        void ExceptionCaught(Exception ex);
        void Dispose();

        void Wait();
        System.Threading.Tasks.Task Task { get; }

    }
}
