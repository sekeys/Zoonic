using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Entities
{
    public class ApiEntityResult<T>:EntityResultBase<T>,IEntityResult<T>
    {
        public string ApiInterface { get; set; }
        public string Token { get; set; }
        public long Expiration { get; set; }
        public string Tag { get; set; }

    }
}
