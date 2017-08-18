using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public abstract class StatePipeline : AbstractPipeline
    {
        public StatePipeline():base(new IndependentThreadExecutor("",null))
        {
            SetHead(new StateHeadContext(DefaultExecutor, this));
            SetTail(new StateTailContext(DefaultExecutor, this));
            //this.head = head;
            //this.tail = tail;
            //this.head.Nxt = this.tail;
            //this.tail.Prev = this.head;
            //DefaultExecutor = new IndependentThreadExecutor(null);
        }
        sealed class StateHeadContext : HeadContext
        {
            public StateHeadContext(IExecutor executor, IPipeline pipeline) : base(executor, pipeline)
            {
                this.Handler = new ActionHandler(() =>
                {
                    ((StatePipeline)pipeline).State = PipelineState.Running;
                    this.Completed();
                });
            }
            public override void Handle()
            {
                this.Handler.Handle();
            }
        }
        sealed class StateTailContext : TailContext
        {
            public StateTailContext(IExecutor executor, IPipeline pipeline) : base(executor, pipeline)
            {
                this.Handler = new ActionHandler(() =>
                {
                    ((StatePipeline)pipeline).State = PipelineState.Completed;
                    this.Completed();

                });
            }
        }

        public PipelineState State
        {
            get;
            protected set; // => Storage.Set(STORAGEPIPRLINENAME, value); 
        }

        //public virtual void Start()
        //{
        //    this.head.Handle();
        //}

        public virtual void Cancel()
        {
            this.State = PipelineState.Canceled;
        }

        public virtual void Completed()
        {
            this.State = PipelineState.Completed;
        }
        

        public override void Next(IHandlerContext context)
        {
            lock (this)
            {
                if (PipelineState.Running == State
                    || PipelineState.Unstarted == State
                    )
                {
                    if (DefaultExecutor.InLoop)
                    {
                        DefaultExecutor.Execute(() =>
                        {
                            var handler = context.Next();
                            handler.Handle();
                        });
                    }
                    else
                    {
                        var handler = context.Next();
                        handler.Handle();
                    }
                }
            }
        }
    }
}
