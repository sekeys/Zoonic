using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Entities
{
    public class EntityResultBase : IEntityResult
    {
        public bool Result { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public object Response()
        {
            return Data;
        }

        public T Response<T>()
        {
            return (T)Data;
        }
    }
    public class EntityResultBase<T> : EntityResultBase, IEntityResult<T>
    {
        public new T Data { get; set; }

    }
}
