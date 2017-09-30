using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Zoonic;

namespace Zoonic.Concurrency
{
    public class StaticPipeline : AbstractPipeline
    {
        class PipelineScopeValue
        {
            public TaskCompletionSource TaskCompletionSource { get; set; }
            public PipelineState State { get; set; }
        }
        readonly Action<Exception> OnException;
        readonly Action OnCompleted;
        readonly Action OnCancel;
        const string STATICPIPELINEDEFAULTEXECUTOR = "STATICPIPELINE.DEFAULTEXECUTOR";
        readonly Func<IExecutor> ExecutorFactory = () => new IndependentThreadExecutor(null, STATICPIPELINEDEFAULTEXECUTOR, TimeSpan.Zero);
        //new ThreadDisposedExecutor(() =>
        //{
        //    AccessorContext.DefaultContext.Remove(STATICPIPELINEDEFAULTEXECUTOR) ;
        //});

        static object Locker = new object();
        public static Func<bool> Filter { get; set; } = () =>
        {
            return Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.IndexOf("Master", StringComparison.OrdinalIgnoreCase) > 0;
        };
        protected override IExecutor DefaultExecutor
        {
            get
            {
                if (Filter()) { return null; }
                var executor = Accessor<IExecutor>.Current; //AccessorContext.DefaultContext.Get<IExecutor>(STATICPIPELINEDEFAULTEXECUTOR);
                if (executor == null)
                {
                    lock (this)
                    {
                        executor = ExecutorFactory();
                        Accessor<IExecutor>.Current = executor;//AccessorContext.DefaultContext.Set<IExecutor>(STATICPIPELINEDEFAULTEXECUTOR, executor);
                    }
                }
                return executor;
                //return null;
            }
        }
        private PipelineScopeValue ScopeValue
        {
            get
            {
                var value = Accessor<PipelineScopeValue>.Current; //AccessorContext.DefaultContext.Get<IExecutor>(STATICPIPELINEDEFAULTEXECUTOR);
                if (value == null)
                {
                    lock (this)
                    {
                        value = new PipelineScopeValue()
                        {
                            State = PipelineState.Unstarted
                        };
                        Accessor<PipelineScopeValue>.Current = value;//AccessorContext.DefaultContext.Set<IExecutor>(STATICPIPELINEDEFAULTEXECUTOR, executor);
                    }
                }
                return value;
            }
        }
        public StaticPipeline(Action completed, Action cancel, Action<Exception> exception)
        {
            OnException = exception;
            OnCompleted = completed;
            OnCancel = cancel;
        }
        public override void ExceptionCaught(Exception ex)
        {
            State = PipelineState.Exception;
            OnException(ex);
            TerminationCompletionSource.TrySetException(ex);
        }
        public override void Start()
        {
            var term = new TaskCompletionSource();
            try
            {
                //AccessorContext.DefaultContext.Set<TaskCompletionSource>(TERMINATIONCOMPLETIONSOURCENAME, term);
                ScopeValue.TaskCompletionSource = term;
                State = PipelineState.Running;
                this.StartCore(DefaultExecutor);
            }
            catch (Exception ex)
            {
                term.SetException(ex);
            }
        }

        public override void Next(IHandlerContext context)
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

                            var pipelineState = PipelineState;
                            if (handler == null && !(pipelineState == PipelineState.Canceled || pipelineState == PipelineState.Completed || pipelineState == PipelineState.Exception))
                            {
                                Completed();
                                return;
                            }
                            handler.Handle(DefaultExecutor);
                        }
                        else
                        {
                            DefaultExecutor.Execute(() =>
                            {
                                var handler = context.Next();
                                var pipelineState = PipelineState;
                                if (handler == null && !(pipelineState == PipelineState.Canceled || pipelineState == PipelineState.Completed 
                                || pipelineState == PipelineState.Exception))
                                {
                                    Completed();
                                    return;
                                }
                                handler.Handle(DefaultExecutor);
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
        //protected override void StartCore()
        //{

        //}
        public override PipelineState State
        {
            get => ScopeValue.State;//AccessorContext.DefaultContext.Get<PipelineState>();
            protected set => ScopeValue.State = value;//AccessorContext.DefaultContext.Set<PipelineState>(value);
        }

        //readonly string TERMINATIONCOMPLETIONSOURCENAME = "STATICPIPELINE.TERMINATIONCOMPLETIONSOURCE";
        protected override TaskCompletionSource TerminationCompletionSource
        {

            get => ScopeValue.TaskCompletionSource; //AccessorContext.DefaultContext.Get<TaskCompletionSource>(TERMINATIONCOMPLETIONSOURCENAME);


        }
        public override void Completed()
        {
            State = PipelineState.Completed;
            OnCompleted();
            TerminationCompletionSource.Complete();
        }
        public override void Wait()
        {
            var executor = DefaultExecutor;
            executor.ShutdownGracefullyAsync(TimeSpan.Zero, TimeSpan.Zero).Wait();
            TerminationCompletionSource.Task.Wait();
            //AccessorContext.DefaultContext.Set<IExecutor>(STATICPIPELINEDEFAULTEXECUTOR, null);
        }
        public override void Cancel()
        {
            State = PipelineState.Canceled;
            OnCancel();
            TerminationCompletionSource.SetCanceled();
        }

        static IPipeline _Static;
        public static IPipeline Static
        {
            get
            {
                return _Static;
            }
        }
        public static PipelineState PipelineState
        {
            get => Accessor<PipelineScopeValue>.Current.State;//AccessorContext.DefaultContext.Get<PipelineState>();
            protected set => Accessor<PipelineScopeValue>.Current.State = value;//AccessorContext.DefaultContext.Set<PipelineState>(value);
        }
        public static void Register(IPipeline pipeline)
        {
            _Static = pipeline;
        }
    }
}
