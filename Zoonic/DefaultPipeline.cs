using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public class DefaultPipeline : AbstractPipeline
    {

        readonly Action<Exception> OnException;
        readonly Action OnCompleted;
        readonly Action OnCancel;
        public DefaultPipeline(Action completed,Action cancel,Action<Exception> exception)
        {
            OnException = exception;
            OnCompleted = completed;
            OnCancel = cancel;
        }
        public override void ExceptionCaught(Exception ex)
        {
            OnException(ex);
        }

        public override void Completed()
        {
            base.Completed();
            OnCompleted();
        }
        public override void Cancel()
        {
            base.Cancel();
            OnCancel();
        }
        
    }
}
