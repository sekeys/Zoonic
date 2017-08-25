

namespace Zoonic.Concurrency
{
    using System;
    using System.Threading.Tasks;
    using Zoonic.Concurrency;
    using System.Threading;
    using System.Linq;

    public class MultiExecutor : IExecutorGroup
    {
        static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount * 2;
        static readonly Func<IExecutorGroup, IExecutor> DefaultEventLoopFactory = group => new IndependentThreadExecutor_out(group);

        readonly IExecutor[] eventLoops;
        int requestId;
        
        public MultiExecutor()
            : this(DefaultEventLoopFactory, DefaultEventLoopThreadCount)
        {
        }
        
        public MultiExecutor(int eventLoopCount)
            : this(DefaultEventLoopFactory, eventLoopCount)
        {
        }
        
        public MultiExecutor(Func<IExecutorGroup, IExecutor> eventLoopFactory)
            : this(eventLoopFactory, DefaultEventLoopThreadCount)
        {
        }
        
        public MultiExecutor(Func<IExecutorGroup, IExecutor> eventLoopFactory, int eventLoopCount)
        {
            this.eventLoops = new IExecutor[eventLoopCount];
            var terminationTasks = new Task[eventLoopCount];
            for (int i = 0; i < eventLoopCount; i++)
            {
                IExecutor eventLoop;
                bool success = false;
                try
                {
                    eventLoop = eventLoopFactory(this);
                    success = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("failed to create a child event loop.", ex);
                }
                finally
                {
                    if (!success)
                    {
                        Task.WhenAll(this.eventLoops
                                .Take(i)
                                .Select(loop => loop.ShutdownGracefullyAsync()))
                            .Wait();
                    }
                }

                this.eventLoops[i] = eventLoop;
                terminationTasks[i] = eventLoop.TerminationCompletion;
            }
            this.TerminationCompletion = Task.WhenAll(terminationTasks);
        }

        
        public Task TerminationCompletion { get; }
        
        public IExecutor GetNext()
        {
            int id = Interlocked.Increment(ref this.requestId);
            return this.eventLoops[Math.Abs(id % this.eventLoops.Length)];
        }

        
        public Task ShutdownGracefullyAsync()
        {
            foreach (IExecutor eventLoop in this.eventLoops)
            {
                eventLoop.ShutdownGracefullyAsync();
            }
            return this.TerminationCompletion;
        }
        
        public Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            foreach (IExecutor eventLoop in this.eventLoops)
            {
                eventLoop.ShutdownGracefullyAsync(quietPeriod, timeout);
            }
            return this.TerminationCompletion;
        }
    }
}
