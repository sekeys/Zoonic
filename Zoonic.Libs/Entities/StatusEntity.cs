using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Entities
{
    public class StatusEntity : IEntityResult<string>
    {
        public bool Result { get { return false; } set { } }
        public int StatusCode { get; set; }
        public string Data { get ; set ; }

        public object Response()
        {
            return Data;
        }

        public T Response<T>()
        {
            
            return default(T);
        }
    }
}
