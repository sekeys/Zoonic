using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Concurrency
{
    public class ThreadDisposedExecutor: IndependentThreadExecutor, IDisposable
    {
        readonly Action Action;
        public ThreadDisposedExecutor() : base(null, "IndependentThreadExecutor.Default", TimeSpan.Zero)
        {
            Action = () => { };
        }
        public ThreadDisposedExecutor(Action action) : base(null, "IndependentThreadExecutor.Default", TimeSpan.Zero)
        {
            Action = action;
        }

        public ThreadDisposedExecutor(Action action, IExecutorGroup parent) : base(parent, "IndependentThreadExecutor.Default", TimeSpan.Zero)
        {
            Action = action;
        }
        public ThreadDisposedExecutor(IExecutorGroup parent) : base(parent, "IndependentThreadExecutor.Default", TimeSpan.Zero)
        {

            Action = () => { };
        }
        //public override Shut()
        //{
        //    this.Schedule
        //}
        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            Action();
            Dispose();
            return base.ShutdownGracefullyAsync(quietPeriod, timeout);
        }
        //public override 
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    GC.Collect();
                }

                disposedValue = true;
            }
        }
        

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
