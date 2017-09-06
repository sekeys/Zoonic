using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web
{
    public class JsonResult : ProcessorResult
    {
        public string ContentType { get; set; }// = "application/json";
        public object Data { get; set; }
        public Encoding Encoding { get; set; }
        public JsonResult(object data=null,string contentType= "application/json"
            ,Encoding encoding=null)
        {
            Data = data;
            ContentType = contentType;
            Encoding =encoding==null? Encoding.UTF8: encoding;
        }
        public override async Task Execute()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this.Data);
            await HttpContext.Response.WriteAsync(json, Encoding);

        }
    }
}
