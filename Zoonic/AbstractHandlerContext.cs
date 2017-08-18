

namespace Zoonic.Concurrency
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    public abstract class AbstractHandlerContext : IHandlerContext
    {

        public AbstractHandlerContext(IExecutor executor, IPipeline pipeline, IHandler handler, string name)
        {
            this.pipeline = pipeline;
            Handler = handler;
            Name = name;
            Executor = executor;
            ExtractAttribute();
        }
        public AbstractHandlerContext(IExecutor executor, IPipeline pipeline)
        {
            this.pipeline = pipeline;
            Executor = executor;
            //ExtractAttribute();
        }
        protected void ExtractAttribute()
        {
            var type = Handler.GetType();
            attributes = type.GetTypeInfo().GetCustomAttributes(true) as IEnumerable<Attribute>;
        }
        private readonly IPipeline pipeline;
        public IPipeline Pipeline => pipeline;

        public IExecutor Executor { get; private set; }

        public string Name { get; protected set; }

        public IHandler Handler { get; protected set; }

        protected IEnumerable<Attribute> attributes;
        public IEnumerable<Attribute> Attributes => attributes;

        //public HandlerState State { get; private set; } = HandlerState.Initilized;
        //internal void SetFailed() => State = HandlerState.Failed;
        //internal void SetCompleted() => State = HandlerState.Completed;
        //internal void SetRunning() => State = HandlerState.Running;
        public virtual void Completed()
        {
            if (this.Executor == null || this.Executor.InLoop)
            {
                pipeline.Next(this);
            }
            else
                this.Executor.Execute(c => { ((AbstractHandlerContext)c).SafeCompleted(); }, this);

        }

        void SafeCompleted()
        {
            try
            {
                this.pipeline.Next(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
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
        protected abstract void HandleCore(IExecutor executor);

        public virtual void ExceptionCaught(Exception ex)
        {
            if (this.Executor == null || this.Executor.InLoop)
                pipeline.ExceptionCaught(ex);
            else
                this.Executor.Execute(c => { ((AbstractHandlerContext)c).SafeExceptionCaught(ex); }, this);
        }
        void SafeExceptionCaught(Exception ex)
        {
            try
            {
                this.pipeline.ExceptionCaught(ex);
            }
            catch (Exception)
            {
            }
        }

        public virtual void Cancel()
        {
            if (this.Executor == null || this.Executor.InLoop)
                pipeline.Cancel();
            else
                this.Executor.Execute(c => { ((AbstractHandlerContext)c).SafeCancel(); }, this);
        }

        void SafeCancel()
        {
            try
            {
                this.pipeline.Cancel();
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
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
                    HandleCore(executor);
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
