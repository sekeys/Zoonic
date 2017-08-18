

namespace Zoonic.Concurrency
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Zoonic.Lib;

    public abstract class AbstractPipeline : IPipeline
    {
        internal class HandlerContext : AbstractHandlerContext
        {
            public volatile HandlerContext Prev;
            public volatile HandlerContext Nxt;


            public HandlerContext(IExecutor executor, IPipeline pipeline, IHandler handler, string name) : base(executor, pipeline, handler, name)
            {
            }
            public HandlerContext(IExecutor executor, IPipeline pipeline, IHandler handler) : base(executor, pipeline, handler, null)
            {
            }

            public HandlerContext(IExecutor executor, IPipeline pipeline) : base(executor, pipeline)
            {
            }
            public HandlerContext(IExecutor executor, IPipeline pipeline, Action action) : base(executor, pipeline)
            {
                this.Handler = new ActionHandler(action);
            }
            public IEnumerable<Attribute> Attributes { get; set; }

            protected override void HandleCore()
            {
                if (Executor == null || Executor.InLoop)
                {
                    try
                    {
                        Handler.Handle();
                        Completed();
                    }
                    catch (Exception ex)
                    {
                        ExceptionCaught(ex);
                    }
                }
                else
                {
                    Executor.Execute(() =>
                    {
                        try
                        {
                            Handler.Handle();
                            Completed();
                        }
                        catch (Exception ex)
                        {
                            ExceptionCaught(ex);
                        }
                    });
                }
            }

            public override IHandlerContext Next()
            {
                return Nxt;
            }

            protected override void HandleCore(IExecutor executor)
            {
                if (executor == null) { HandleCore(); }
                if (executor != null || executor.InLoop)
                {
                    try
                    {
                        Handler.Handle();
                        Completed();
                    }
                    catch (Exception ex)
                    {
                        ExceptionCaught(ex);
                    }
                }
                else
                {
                    executor.Execute(() =>
                    {
                        try
                        {
                            Handler.Handle();
                            Completed();
                        }
                        catch (Exception ex)
                        {
                            ExceptionCaught(ex);
                        }
                    });
                }
            }
        }

        internal class HeadContext : HandlerContext
        {
            public HeadContext(IExecutor executor, IPipeline pipeline) : base(executor, pipeline)
            {
                this.Handler = new ActionHandler(() =>
                {
                    ((AbstractPipeline)pipeline).State = PipelineState.Running;
                    
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
            public TailContext(IExecutor executor, IPipeline pipeline) : base(executor, pipeline)
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
        private readonly IExecutor _DefaultExecutor;
        protected virtual IExecutor DefaultExecutor { get => _DefaultExecutor; }
        //const string ACCESSORCONTEXTSTORAGENAME = "STORAGEABSTRACTPIPELINE.STORAGEACCESSOR";
        //const string STORAGEPIPRLINENAME = "STORAGE.PIPELINESTATE";
        //public static AccessorContext Storage => AccessorContext.Gets<AccessorContext>(ACCESSORCONTEXTSTORAGENAME);


        private HeadContext head;
        private TailContext tail;

        readonly TaskCompletionSource terminationCompletionSource;
        protected virtual TaskCompletionSource TerminationCompletionSource => terminationCompletionSource;
        private Dictionary<IExecutorGroup, IExecutor> childExecutors;
        //internal AbstractPipeline(HeadContext head, TailContext tail)
        //{
        //    this.head = head;
        //    this.tail = tail;
        //    this.head.Nxt = this.tail;
        //    this.tail.Prev = this.head;
        //    DefaultExecutor = new IndependentThreadExecutor(null);
        //}
        internal AbstractPipeline(IExecutor executor)
        {
            terminationCompletionSource = new TaskCompletionSource();
            _DefaultExecutor = executor;
        }
        internal void SetHead(HeadContext head)
        {
            var nxt = this.head.Nxt;
            this.head = head;
            this.head.Nxt = nxt;
            this.head.Prev = null;
        }
        internal void SetTail(TailContext tail)
        {
            var prev = this.tail.Prev;
            this.tail = tail;

            this.tail.Prev = prev;
            this.tail.Nxt = null;
        }
        public AbstractPipeline()
        {
            head = new HeadContext(DefaultExecutor, this);
            tail = new TailContext(DefaultExecutor, this);
            this.head.Nxt = this.tail;
            this.tail.Prev = this.head;
            _DefaultExecutor = new IndependentThreadExecutor(null, "IndependentThreadExecutor Worker", TimeSpan.Zero);
            //new AccessorContext(ACCESSORCONTEXTSTORAGENAME);//生成线程存储空间
        }
        HandlerContext NewContext(string name, IHandler handler)
        {
            return new HandlerContext(DefaultExecutor, this, handler, name);
        }
        HandlerContext NewContext(IExecutor executor, string name, IHandler handler)
        {
            return new HandlerContext(executor, this, handler, name);
        }
        IExecutor GetChildExecutor(IExecutorGroup group)
        {
            if (group == null)
            {
                return null;
            }
            // Use size of 4 as most people only use one extra EventExecutor.
            Dictionary<IExecutorGroup, IExecutor> executorMap = this.childExecutors
                ?? (this.childExecutors = new Dictionary<IExecutorGroup, IExecutor>(4, ReferenceEqualityComparer.Default));

            // Pin one of the child executors once and remember it so that the same child executor
            // is used to fire events for the same channel.
            IExecutor childExecutor;
            if (!executorMap.TryGetValue(group, out childExecutor))
            {
                childExecutor = group.GetNext();
                executorMap[group] = childExecutor;
            }
            return childExecutor;
        }
        public IPipeline AddAfter(string beforename, string name, IHandler handler) => AddAfter(null, beforename, name, handler);
        static void AddBefore0(HandlerContext ctx, HandlerContext newCtx)
        {
            newCtx.Prev = ctx.Prev;
            newCtx.Nxt = ctx;
            ctx.Prev.Nxt = newCtx;
            ctx.Prev = newCtx;
        }

        public IPipeline AddBefore(IExecutorGroup group, string aftername, string name, IHandler handler)
        {
            var executor = GetChildExecutor(group) ?? DefaultExecutor;
            lock (this)
            {
                var cur = NewContext(executor, name, handler);
                var ctx = (HandlerContext)Context(aftername);
                AddBefore0(ctx, cur);
            }
            return this;
        }
        public IPipeline AddAfter(IExecutorGroup group, string beforeName, string name, IHandler handler)
        {
            var executor = GetChildExecutor(group) ?? DefaultExecutor;
            lock (this)
            {
                var cur = NewContext(executor, name, handler);
                var ctx = (HandlerContext)Context(beforeName);
                var next = ctx.Nxt;
                AddAfter0(ctx, cur);
            }
            return this;
        }
        static void AddAfter0(HandlerContext ctx, HandlerContext newCtx)
        {
            newCtx.Prev = ctx;
            newCtx.Nxt = ctx.Nxt;
            ctx.Nxt.Prev = newCtx;
            ctx.Nxt = newCtx;
        }
        public IPipeline AddBefore(string aftername, string name, IHandler handler) => AddBefore(null, aftername, name, handler);

        public IPipeline AddFirst(string name, IHandler handler) => AddFirst(null, name, handler);
        void AddFirst0(HandlerContext newCtx)
        {
            HandlerContext nextCtx = this.head.Nxt;
            newCtx.Prev = this.head;
            newCtx.Nxt = nextCtx;
            this.head.Nxt = newCtx;
            nextCtx.Prev = newCtx;
        }
        public IPipeline AddFirst(IExecutorGroup group, string name, IHandler handler)
        {
            Require.NotNull(handler);
            var executor = GetChildExecutor(group) ?? DefaultExecutor;

            AddFirst0(new HandlerContext(executor, this, handler, name));

            return this;
        }

        public IPipeline AddFirst(params IHandler[] handlers) => AddFirst(null, handlers);

        public IPipeline AddFirst(IExecutorGroup group, params IHandler[] handlers)
        {
            Require.NotNull(handlers);

            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                var h = handlers[i];
                this.AddFirst(group, (string)null, h);
            }

            return this;
        }

        public IPipeline AddLast(string name, IHandler handler) => AddLast(null, name, handler);

        public IPipeline AddLast(IExecutorGroup group, string name, IHandler handler)
        {
            Require.NotNull(handler);
            var executor = GetChildExecutor(group) ?? DefaultExecutor;

            lock (this)
            {
                AddLast0(new HandlerContext(executor, this, handler, name));
            }
            return this;
        }

        public IPipeline AddLast(IExecutorGroup group, string beforeName, string name, IHandler handler)
        {
            throw new NotImplementedException();
        }

        public IPipeline AddLast(params IHandler[] handlers) => AddLast(null, handlers);

        public IPipeline AddLast(IExecutorGroup group, params IHandler[] handlers)
        {
            foreach (var h in handlers)
            {
                this.AddLast(group, (string)null, h);
            }
            return this;
        }
        void AddLast0(HandlerContext newCtx)
        {
            HandlerContext prev = this.tail.Prev;
            newCtx.Prev = prev;
            newCtx.Nxt = this.tail;
            prev.Nxt = newCtx;
            this.tail.Prev = newCtx;
        }
        private bool Disposed = false;

        public virtual PipelineState State { get; protected set; }

        public Task Task => terminationCompletionSource.Task;

        public void Dispose()
        {
            if (!Disposed)
            {
                GC.Collect();
            }
            //Storage.Remove();
            //Storage.Dispose();
        }


        public IHandler First()
        {
            HandlerContext first = this.head.Nxt;
            return first == this.tail ? default(IHandler) : first.Handler;
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

        public T Get<T>() where T : class
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
                    return ctx.Handler as T;
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
        public IPipeline Remove(IHandler handler)
        {
            var executor = this.GetChildExecutor(null);
            var ctx = Context(handler);
            lock (this)
            {
                Remove0((HandlerContext)ctx);
            }
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
            lock (this)
            {
                Remove0((HandlerContext)ctx);
            }
            return ctx.Handler;
        }
        HandlerContext Remove(HandlerContext ctx)
        {
            lock (this)
            {
                Remove0(ctx);
            }
            return ctx;
        }

        public T1 Remove<T1>() where T1 : class, IHandler
        {
            var ctx = Context<T1>();
            lock (this)
            {
                Remove0((HandlerContext)ctx);
            }
            return ctx.Handler as T1;
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
            var executor = DefaultExecutor;
            lock (this)
            {
                var ctx = Context(old);
                Replace0((HandlerContext)ctx, new HandlerContext(executor, this, newValue, newName));
            }
            return this;
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
        public IPipeline Replace(string old, string newName, IHandler newValue)
        {
            var executor = DefaultExecutor;
            lock (this)
            {
                var ctx = Context(old);
                Replace0((HandlerContext)ctx, new HandlerContext(DefaultExecutor, this, newValue, newName));
            }
            return this;
        }

        public IPipeline Replace<T1>(string newName, IHandler newValue) where T1 : class, IHandler
        {
            var executor = DefaultExecutor;
            executor.Execute(() =>
            {
                lock (this)
                {
                    var ctx = Context<T1>();
                    Replace0((HandlerContext)ctx, new HandlerContext(DefaultExecutor, this, newValue));
                }
            });
            return this;
        }


        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IHandler>)this).GetEnumerator();

        protected void StartCore()
        {
            this.head.Handle();
        }
        protected void StartCore(IExecutor executor)
        {
            this.head.Handle(executor);
        }
        public virtual void Start()
        {
            try
            {
                this.head.Handle();
            }
            catch (Exception ex)
            {
                terminationCompletionSource.SetException(ex);
            }
        }

        public virtual void Cancel()
        {
            this.State = PipelineState.Canceled;
        }

        public virtual void Completed()
        {
            this.State = PipelineState.Completed;
            TerminationCompletionSource.Complete();
        }
        public virtual void Wait()
        {
            TerminationCompletionSource.Task.Wait();
        }

        public abstract void ExceptionCaught(Exception ex);

        public virtual void Next(IHandlerContext context)
        {
            try
            {
                lock (this)
                {
                    if (PipelineState.Running == State
                        || PipelineState.Unstarted == State
                        )
                    {
                        if (DefaultExecutor.InLoop)
                        {
                            var handler = context.Next();
                            if (handler == null)
                                Completed();
                            handler.Handle();
                        }
                        else
                        {
                            DefaultExecutor.Execute(() =>
                            {
                                var handler = context.Next();
                                if (handler == null)
                                    Completed();
                                handler.Handle();
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionCaught(ex);
            }
        }


        #region
        //public virtual IPipeline AddFirst(string name,Action action)
        //{

        //}

        //IPipeline AddFirst(IExecutorGroup group, string name, IHandler value);

        //IPipeline AddLast(string name, IHandler value);

        //IPipeline AddLast(IExecutorGroup group, string name, IHandler value);


        //IPipeline AddBefore(string beForename, string name, IHandler value);

        //IPipeline AddLast(IExecutorGroup group, string beforeName, string name, IHandler value);


        //IPipeline AddAfter(string beForename, string name, IHandler value);

        //IPipeline AddAfter(IExecutorGroup group, string beforeName, string name, IHandler value);

        //IPipeline AddFirst(params IHandler[] values);
        //IPipeline AddFirst(IExecutorGroup group, params IHandler[] values);


        //IPipeline AddLast(params IHandler[] values);
        //IPipeline AddLast(IExecutorGroup group, params IHandler[] values);
        #endregion
    }


}
