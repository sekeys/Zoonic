using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web
{
    public class ContentResult : ProcessorResult
    {
        public string ContentType { get; set; }// = "application/json";
        public string Content { get; set; }
        public Encoding Encoding { get; set; }
        public ContentResult(string content, string contentType = "application/json"
            , Encoding encoding = null)
        {
            Content = content;
            ContentType = contentType;
            Encoding = encoding == null ? Encoding.UTF8 : encoding;
        }
        public override async Task Execute()
        {
            await HttpContext.Response.WriteAsync(Content, Encoding);

        }
    }
}
