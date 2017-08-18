using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public interface IHandlerContext
    {
        IExecutor Executor { get; }
        string Name { get; }
        IHandler Handler { get; }
        IEnumerable<Attribute> Attributes { get; }

        IPipeline Pipeline { get; }
        IHandlerContext Next();
        void Handle();
        void Handle(IExecutor executor);
        void Completed();

        void ExceptionCaught(Exception ex);

        void Cancel();
        
    }
}
