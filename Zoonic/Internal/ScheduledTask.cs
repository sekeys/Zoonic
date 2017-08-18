﻿
namespace Zoonic.Concurrency
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Zoonic.Lib;

    abstract class ScheduledTask : IScheduledRunnable
    {
        const int CancellationProhibited = 1;
        const int CancellationRequested = 1 << 1;

        protected readonly TaskCompletionSource Promise;
        protected readonly Executor Executor;
        int volatileCancellationState;

        protected ScheduledTask(Executor executor, PreciseTimeSpan deadline, TaskCompletionSource promise)
        {
            this.Executor = executor;
            this.Promise = promise;
            this.Deadline = deadline;
        }

        public PreciseTimeSpan Deadline { get; }

        public bool Cancel()
        {
            if (!this.AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested))
            {
                return false;
            }

            bool canceled = this.Promise.TrySetCanceled();
            if (canceled)
            {
                this.Executor.RemoveScheduled(this);
            }
            return canceled;
        }

        public Task Completion => this.Promise.Task;

        public TaskAwaiter GetAwaiter() => this.Completion.GetAwaiter();

        int IComparable<IScheduledRunnable>.CompareTo(IScheduledRunnable other)
        {
            Require.NotNull(other);

            return this.Deadline.CompareTo(other.Deadline);
        }

        public virtual void Run()
        {
            if (this.TrySetUncancelable())
            {
                try
                {
                    this.Execute();
                    this.Promise.TryComplete();
                }
                catch (Exception ex)
                {
                    // todo: check for fatal
                    this.Promise.TrySetException(ex);
                }
            }
        }

        protected abstract void Execute();

        bool TrySetUncancelable() => this.AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);

        bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
        {
            int cancellationState = Volatile.Read(ref this.volatileCancellationState);
            int oldCancellationState;
            do
            {
                oldCancellationState = cancellationState;
                if ((cancellationState & illegalBits) != 0)
                {
                    return false;
                }
                cancellationState = Interlocked.CompareExchange(ref this.volatileCancellationState, cancellationState | newBits, cancellationState);
            }
            while (cancellationState != oldCancellationState);

            return true;
        }
    }
}
