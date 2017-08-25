
namespace Zoonic.Concurrency
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public abstract class SynchronizationHandlerContext : IHandlerContext
    {
        public SynchronizationHandlerContext(IPipeline pipeline, IHandler handler, string name)
        {
            this.pipeline = pipeline;
            Handler = handler;
            Name = name;
            ExtractAttribute();
        }
        public SynchronizationHandlerContext(IPipeline pipeline)
        {
            this.pipeline = pipeline;
            //ExtractAttribute();
        }
        protected void ExtractAttribute()
        {
            var type = Handler.GetType();
            attributes = type.GetTypeInfo().GetCustomAttributes(true) as IEnumerable<Attribute>;
        }
        private readonly IPipeline pipeline;
        public IPipeline Pipeline => pipeline;

        public IExecutor Executor => null;

        public string Name { get; protected set; }

        public IHandler Handler { get; protected set; }

        protected IEnumerable<Attribute> attributes;
        public IEnumerable<Attribute> Attributes => attributes;
        
        public virtual void Completed()
        {
            if(pipeline.State== PipelineState.Canceled
                || pipeline.State== PipelineState.Exception
                || pipeline.State== PipelineState.Completed)
            {
                return;
            }
            pipeline.Next(this);
        }


        public IEnumerable<T> GetAttribute<T>() where T : class => Attributes.Where(m => m is T).Select(m => m as T);
        public virtual void Handle()
        {
            if (this.pipeline.State == PipelineState.Running)
            {
                try
                {
                    HandleCore();
                    //Handler.Handle();
                    //Completed();
                }
                catch (Exception ex)
                {
                    ExceptionCaught(ex);
                }
            }
            else
            {
                throw new Exception("该Handler出现在意外位置，关联Pipeline并已关闭执行");
            }
        }
        protected abstract void HandleCore();
       

        public virtual void ExceptionCaught(Exception ex)
        {
            pipeline.ExceptionCaught(ex);
        }
        public virtual void Cancel()
        {
            pipeline.Cancel();
        }



        protected void NotifyHandlerException(Exception ex)
        {
            this.ExceptionCaught(ex);
        }

        public abstract IHandlerContext Next();

        public void Handle(IExecutor executor)
        {
            if (this.pipeline.State == PipelineState.Running)
            {
                try
                {
                    HandleCore();
                    //Handler.Handle();
                    //Completed();
                }
                catch (Exception ex)
                {
                    ExceptionCaught(ex);
                }
            }
            else
            {
                throw new Exception("该Handler出现在意外位置，关联Pipeline并已关闭执行");
            }
        }
    }
}
