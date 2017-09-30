using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public class PipelineScopeValue
    {
        public PipelineState State { get; set; }

        public dynamic Parameters { get; set; }
        public PipelineScopeValue()
        {
            State = PipelineState.Unstarted;
            Parameters = new Zoonic.IgnoreDynamic();
        }
        public bool TryGet(string field, out object value)
        {
            if (Parameters.Contains(field))
            {
                value = Parameters[field];
                return true;
            }
            value = null;
            return false;
        }
        public bool TryGet<T>(string field, out T value)
        {
            if (Parameters.Contains(field))
            {
                value = (T)Parameters[field];
                return true;
            }
            value = default(T);
            return false;

        }

        public void TrySet(string field, object value)
        {
            this.Parameters[field] = value;
        }
    }
}
