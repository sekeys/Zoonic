using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web
{
    public class StatusResult : ProcessorResult
    {
        public StatusResult(Exception ex)
        {
            Code = 500;
            Message = ex.Message;
        }
        public StatusResult(int code,Exception ex)
        {
            Code = code;
            Message = ex.Message;
        }
        public StatusResult(int code, string message="")
        {
            Code = code;
            Message = message;
        }
        public string Message { get; set; }
        public int Code { get; set; }
        public override async Task Execute()
        {
            HttpContext.Response.StatusCode = Code;
            await HttpContext.Response.WriteAsync(Message);
        }
    }
}
