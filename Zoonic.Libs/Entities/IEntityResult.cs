using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Entities
{
    public interface IEntityResult
    {
        bool Result { get; set; }
        int StatusCode { get; set; }

        object Response();
        T Response<T>();
    }
    public interface IEntityResult<T>:IEntityResult
    {
        T Data { get; set; }
    }
}
