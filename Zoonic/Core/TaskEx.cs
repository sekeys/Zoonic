﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{

    using System;
    using System.Threading.Tasks;

    public static class TaskEx
    {
        public static readonly Task<int> Zero = Task.FromResult(0);

        public static readonly Task<int> Completed = Zero;

        public static readonly Task<int> Cancelled = CreateCancelledTask();

        static Task<int> CreateCancelledTask()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task FromException(Exception exception)
        {
            var tcs = new TaskCompletionSource();
            tcs.TrySetException(exception);
            return tcs.Task;
        }

        public static Task<T> FromException<T>(Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetException(exception);
            return tcs.Task;
        }

        static readonly Action<Task, object> LinkOutcomeContinuationAction = (t, tcs) =>
        {
            switch (t.Status)
            {
                case TaskStatus.RanToCompletion:
                    ((TaskCompletionSource)tcs).TryComplete();
                    break;
                case TaskStatus.Canceled:
                    ((TaskCompletionSource)tcs).TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    ((TaskCompletionSource)tcs).TrySetException(t.Exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };

        public static void LinkOutcome(this Task task, TaskCompletionSource taskCompletionSource)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    taskCompletionSource.TryComplete();
                    break;
                case TaskStatus.Canceled:
                    taskCompletionSource.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    taskCompletionSource.TrySetException(task.Exception);
                    break;
                default:
                    task.ContinueWith(
                        LinkOutcomeContinuationAction,
                        taskCompletionSource,
                        TaskContinuationOptions.ExecuteSynchronously);
                    break;
            }
        }

        static class LinkOutcomeActionHost<T>
        {
            public static readonly Action<Task<T>, object> Action =
                (t, tcs) =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            ((TaskCompletionSource<T>)tcs).TrySetResult(t.Result);
                            break;
                        case TaskStatus.Canceled:
                            ((TaskCompletionSource<T>)tcs).TrySetCanceled();
                            break;
                        case TaskStatus.Faulted:
                            ((TaskCompletionSource<T>)tcs).TrySetException(t.Exception);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };
        }

        public static void LinkOutcome<T>(this Task<T> task, TaskCompletionSource<T> taskCompletionSource)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    taskCompletionSource.TrySetResult(task.Result);
                    break;
                case TaskStatus.Canceled:
                    taskCompletionSource.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    taskCompletionSource.TrySetException(task.Exception);
                    break;
                default:
                    task.ContinueWith(LinkOutcomeActionHost<T>.Action, taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
                    break;
            }
        }
    }
}
