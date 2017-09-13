using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zoonic.Lib;
using System.Linq;
using Zoonic.Attributes;
using Zoonic.Lib.Authentication;

namespace Zoonic.Concurrency
{
    /// <summary>
    /// 同步执行Pipeline.不涉及Executor 线程问题，无法获取Task
    /// </summary>
    public class SynchronizationPipeline : IPipeline
    {
        public static PipelineScopeValue ScopeValue
        {
            get
            {
                return Accessor<PipelineScopeValue>.Current;
            }
        }
        internal class HandlerContext : SynchronizationHandlerContext
        {
            public HandlerContext Prev;
            public HandlerContext Nxt;
            public bool SkipAuthentication { get; private set; } = false;
            public HandlerContext(IPipeline pipeline, IHandler handler, string name) : base(pipeline, handler, name)
            {
                this.Attributes = handler.GetType().GetCustomAttributes(false) as IEnumerable<Attribute>;
                if (this.Attributes != null && this.Attributes.Count() > 0)
                {
                    var skipAuthenAttr = this.Attributes.Where(m => m is SkipAuthenticationAttribute).FirstOrDefault();
                    if (skipAuthenAttr != null)
                    {
                        SkipAuthentication = true;
                    }
                }
                else
                {
                    SkipAuthentication = true;
                }


            }
            public HandlerContext(IPipeline pipeline, IHandler handler) : base(pipeline, handler, null)
            {
            }

            public HandlerContext(IPipeline pipeline) : base(pipeline)
            {
            }
            public HandlerContext(IPipeline pipeline, Action action) : base(pipeline)
            {
                this.Handler = new ActionHandler(action);
            }
            public IEnumerable<Attribute> Attributes { get; private set; } = new List<Attribute>();

            protected override void HandleCore()
            {
                try
                {
                    if (!this.SkipAuthentication)
                    {
                        IUser user = null;
                        if (ScopeValue.TryGet<IUser>("user", out user) && this.Attributes != null)
                        {
                            foreach (IAuthenticationAttribute item in this.Attributes.Where(m => m is IAuthenticationAttribute))
                            {
                                item.Authorized(user);
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    Task.Run(() =>
                    {
                        if (this.Attributes != null)
                        {
                            foreach (LogAttribute item in
                            this.Attributes.Where(m => m is LogAttribute))
                            {
                                item.Log();
                            }
                        }
                    });
                    if (this.Attributes != null)
                    {
                        foreach (ExecutingAttribute item in this.Attributes.Where(m => m is ExecutingAttribute)
                            .Select(m => (ExecutingAttribute)m).OrderBy(m => m.Priority))
                        {
                            item.Execute();
                        }
                    }
                    //这里执行代码
                    Handler.Handle();
                    if (this.Attributes != null)
                    {
                        foreach (ExecutedAttribute item in this.Attributes.Where(m => m is ExecutedAttribute)
                            .Select(m => (ExecutedAttribute)m).OrderBy(m => m.Priority))
                        {
                            item.Execute();
                        }
                    }
                    Completed();
                }
                catch (Exception ex)
                {
                    ExceptionCaught(ex);
                }
            }
            public override void Handle()
            {
                base.Handle();
            }

            public override IHandlerContext Next()
            {
                if (this.Attributes != null && this.Attributes.Count() > 0)
                {
                    var InvokeNexts = this.Attributes.Where(m => m is InvokeNextHandlerAttribute) 
                        as IEnumerable<InvokeNextHandlerAttribute>;
                    if (InvokeNexts != null && InvokeNexts.Count() > 0)
                    {
                        IHandler next = null;
                        foreach (var item in InvokeNexts)
                        {
                            next = item.Invoke();
                            if (next != null)
                            {
                                return Wrap(next);
                            }
                        }
                        
                    }
                    var InvokeNextHandlerCs = this.Attributes.Where(m => m is InvokeNextHandlerContextAttribute) 
                        as IEnumerable<InvokeNextHandlerContextAttribute>;
                    if (InvokeNextHandlerCs != null && InvokeNextHandlerCs.Count() > 0)
                    {
                        IHandlerContext next = null;
                        foreach (var item in InvokeNextHandlerCs)
                        {
                            next = item.Invoke();
                            if (next != null)
                            {
                                return next;
                            }
                        }
                    }
                }
                return  Nxt;
            }
            public IHandlerContext Wrap(IHandler handler)
            {
                var hc = new HandlerContext(this.Pipeline, handler, "asyonc_" + Guid.NewGuid().ToString());
                hc.Nxt = this.Nxt;
                hc.Prev = this;
                return hc;
            }
        }

        internal class HeadContext : HandlerContext
        {
            public HeadContext(IPipeline pipeline) : base(pipeline)
            {
                this.Handler = new ActionHandler(() =>
                {
                    ((SynchronizationPipeline)pipeline).pipelineState = PipelineState.Running;
                    Completed();
                });
            }
            public override void Handle()
            {
                this.Handler.Handle();
                //this.Completed();
            }
        }
        internal class TailContext : HandlerContext
        {
            public TailContext(IPipeline pipeline) : base(pipeline)
            {
                this.Handler = new ActionHandler(() =>
                {
                    //((AbstractPipeline)pipeline).State = PipelineState.Completed;
                    //this.Completed();

                });
            }

            public override void Handle()
            {
                //this.Completed();
                this.Handler.Handle();
            }
        }

        private HeadContext head;
        private TailContext tail;
        private PipelineState pipelineState
        {
            get => Zoonic.Lib.Accessor<PipelineScopeValue>.Current.State;
            set
            {
                Accessor<PipelineScopeValue>.Current.State = value;
            }
        }
        public PipelineState State => Accessor<PipelineScopeValue>.Current.State;

        public Task Task => null;
        readonly Action<Exception> OnException;
        public SynchronizationPipeline(Action<Exception> exhandler)
        {
            OnException = exhandler;
            head = new HeadContext(this);
            tail = new TailContext(this);
            this.head.Nxt = this.tail;
            this.tail.Prev = this.head;
        }
        public SynchronizationPipeline() : this((ex) => { throw ex; })
        {
        }
        public IPipeline AddAfter(string beforename, string name, IHandler value)
        => AddAfter(null, beforename, name, value);
        static void AddBefore0(HandlerContext ctx, HandlerContext newCtx)
        {
            newCtx.Prev = ctx.Prev;
            newCtx.Nxt = ctx;
            ctx.Prev.Nxt = newCtx;
            ctx.Prev = newCtx;
        }

        void AddAfter0(HandlerContext ctx, HandlerContext newCtx)
        {
            newCtx.Prev = ctx;
            newCtx.Nxt = ctx.Nxt;
            ctx.Nxt.Prev = newCtx;
            ctx.Nxt = newCtx;
        }
        void AddLast0(HandlerContext newCtx)
        {
            HandlerContext prev = this.tail.Prev;
            newCtx.Prev = prev;
            newCtx.Nxt = this.tail;
            prev.Nxt = newCtx;
            this.tail.Prev = newCtx;
        }
        void AddFirst0(HandlerContext newCtx)
        {
            HandlerContext nextCtx = this.head.Nxt;
            newCtx.Prev = this.head;
            newCtx.Nxt = nextCtx;
            this.head.Nxt = newCtx;
            nextCtx.Prev = newCtx;
        }

        HandlerContext NewContext(string name, IHandler handler)
        {
            return new HandlerContext(this, handler, name);
        }
        public IPipeline AddAfter(IExecutorGroup group, string beforeName, string name, IHandler value)
        {
            return AddAfter(beforeName, name, value);
        }

        public IPipeline AddBefore(string beForename, string name, IHandler value)
        {
            var cur = NewContext(name, value);
            var ctx = (HandlerContext)Context(beForename);
            AddBefore0(ctx, cur);
            return this;
        }

        public IPipeline AddFirst(string Name, IHandler value)
        {
            Require.NotNull(value);

            AddFirst0(new HandlerContext(this, value, Name));
            return this;
        }

        public IPipeline AddFirst(IExecutorGroup group, string name, IHandler value)
        {
            return AddFirst(name, value);
        }

        public IPipeline AddFirst(params IHandler[] values)
        {
            Require.NotNull(values);
            for (int i = values.Length - 1; i >= 0; i--)
            {
                var h = values[i];
                this.AddFirst((string)null, h);
            }

            return this;
        }

        public IPipeline AddFirst(IExecutorGroup group, params IHandler[] values)
        => AddFirst(values);

        public IPipeline AddLast(string name, IHandler value)
        {
            Require.NotNull(value);
            lock (this)
            {
                AddLast0(new HandlerContext(this, value, name));
            }
            return this;
        }

        public IPipeline AddLast(IExecutorGroup group, string name, IHandler value)
        {
            return AddLast(name, value);
        }



        public IPipeline AddLast(params IHandler[] values) => AddLast(null, values);

        public IPipeline AddLast(IExecutorGroup group, params IHandler[] values)
        {
            foreach (var h in values)
            {
                this.AddLast((string)null, h);
            }
            return this;
        }


        public virtual void Cancel()
        {
            this.pipelineState = PipelineState.Canceled;
        }

        static void Replace0(HandlerContext oldCtx, HandlerContext newCtx)
        {
            HandlerContext prev = oldCtx.Prev;
            HandlerContext next = oldCtx.Nxt;
            newCtx.Prev = prev;
            newCtx.Nxt = next;

            // Finish the replacement of oldCtx with newCtx in the linked list.
            // Note that this doesn't mean events will be sent to the new handler immediately
            // because we are currently at the event handler thread and no more than one handler methods can be invoked
            // at the same time (we ensured that in replace().)
            prev.Nxt = newCtx;
            next.Prev = newCtx;

            // update the reference to the replacement so forward of buffered content will work correctly
            oldCtx.Prev = newCtx;
            oldCtx.Nxt = newCtx;
        }
        public virtual void Completed()
        {
            this.pipelineState = PipelineState.Completed;
        }

        public IHandlerContext Context(string name)
        {
            HandlerContext context = this.head.Nxt;
            while (context != this.tail)
            {
                if (context.Name.Equals(name, StringComparison.Ordinal))
                {
                    return context;
                }
                context = context.Nxt;
            }
            return null;
        }

        public IHandlerContext Context<T>()
        {
            HandlerContext ctx = this.head.Nxt;
            while (true)
            {
                if (ctx == null || ctx is TailContext)
                {
                    return null;
                }
                if (ctx.Handler is T)
                {
                    return ctx;
                }
                ctx = ctx.Nxt;
            }
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                GC.Collect();
            }
        }

        private bool Disposed = false;
        public void ExceptionCaught(Exception ex)
        {
            OnException(ex);
        }

        public IHandler First()
        {
            HandlerContext first = this.head.Nxt;
            return first == this.tail ? default(IHandler) : first.Handler;
        }

        public IHandler Get(string name)
        {
            HandlerContext context = this.head.Nxt;
            while (context != this.tail)
            {
                if (context.Name.Equals(name, StringComparison.Ordinal))
                {
                    return context.Handler;
                }
                context = context.Nxt;
            }
            return null;
        }

        public T1 Get<T1>() where T1 : class
        {
            HandlerContext ctx = this.head.Nxt;
            while (true)
            {
                if (ctx == null || ctx is TailContext)
                {
                    return null;
                }
                if (ctx.Handler is T1)
                {
                    return ctx.Handler as T1;
                }
                ctx = ctx.Nxt;
            }
        }

        public IEnumerator<IHandler> GetEnumerator()
        {
            HandlerContext current = this.head;
            while (current != null)
            {
                yield return current.Handler;
                current = current.Nxt;
            }
        }

        public IHandler Last()
        {
            HandlerContext last = this.tail.Prev;
            return last == this.head ? default(IHandler) : last.Handler;
        }
        public IHandlerContext Context(IHandler handler)
        {
            Require.NotNull(handler);

            HandlerContext ctx = this.head.Nxt;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }

                if (ctx.Handler == handler)
                {
                    return ctx;
                }

                ctx = ctx.Nxt;
            }
        }
        public void Next(IHandlerContext context)
        {
            try
            {
                if (PipelineState.Running == State
                    || PipelineState.Unstarted == State
                    )
                {
                    var handler = context.Next();
                    if (handler == null)
                        Completed();

                    handler.Handle();
                }

            }
            catch (Exception ex)
            {
                ExceptionCaught(ex);
            }
        }

        public IPipeline Remove(IHandler value)
        {
            var ctx = Context(value);
            Remove0((HandlerContext)ctx);

            return this;
        }
        static void Remove0(HandlerContext context)
        {
            HandlerContext prev = context.Prev;
            HandlerContext next = context.Nxt;
            prev.Nxt = next;
            next.Prev = prev;
        }
        public IHandler Remove(string name)
        {
            var ctx = Context(name);
            Remove0((HandlerContext)ctx);

            return ctx.Handler;
        }
        HandlerContext Remove(HandlerContext ctx)
        {
            Remove0(ctx);
            return ctx;
        }

        public IHandler RemoveFirst()
        {
            if (this.head.Nxt == this.tail)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return this.Remove(this.head.Nxt).Handler;
        }

        public IHandler RemoveLast()
        {
            if (this.tail.Prev == this.head)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return this.Remove(this.tail.Prev).Handler;
        }

        public IPipeline Replace(IHandler old, string newName, IHandler newValue)
        {
            var ctx = Context(old);
            Replace0((HandlerContext)ctx, new HandlerContext(this, newValue, newName));

            return this;
        }

        public IPipeline Replace(string old, string newName, IHandler newValue)
        {
            var ctx = Context(old);
            Replace0((HandlerContext)ctx, new HandlerContext(this, newValue, newName));

            return this;
        }

        public void Start()
        {
            try
            {
                Accessor<PipelineScopeValue>.Current = new PipelineScopeValue();
                this.head.Handle();
            }
            catch (Exception ex)
            {
                ExceptionCaught(ex);
            }
        }

        public void Wait()
        {

        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IHandler>)this).GetEnumerator();

        public T1 Remove<T1>() where T1 : class, IHandler
        {
            var ctx = Context<T1>();
            lock (this)
            {
                Remove0((HandlerContext)ctx);
            }
            return ctx.Handler as T1;
        }

        public IPipeline Replace<T1>(string newName, IHandler newValue)
            where T1 : class, IHandler
        {

            var ctx = Context<T1>();
            Replace0((HandlerContext)ctx
                , new HandlerContext(this, newValue));

            return this;
        }
    }
}
