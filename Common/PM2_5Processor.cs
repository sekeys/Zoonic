using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoonic.Web;

namespace Common
{
    public class PM2_5Processor : ProcessorBase
    {
        private const string host = "http://jisutqybmf.market.alicloudapi.com/weather/query";
        private const string method = "GET";
        private const string Appcode = "49cc636b374744ea8dfdd81dc102dc62";

        public override async Task<ProcessorResult> Process()
        {
            string city = Request.Query["city"].ToString();
            string cityCode = Request.Query["ccode"].ToString();
            string cityid = Request.Query["cid"].ToString();
            string ip = Request.Query["ip"].ToString();
            string location = Request.Query["loc"].ToString();
            string query = "apcache=" + Guid.NewGuid().ToString();
            if (!string.IsNullOrWhiteSpace(city))
            {
                query += "&city=" + city;
            }
            if (!string.IsNullOrWhiteSpace(cityCode))
            {
                query += "&cityCode=" + cityCode;
            }
            if (!string.IsNullOrWhiteSpace(cityid))
            {
                query += "&cityid=" + cityid;
            }
            if (!string.IsNullOrWhiteSpace(ip))
            {
                query += "&ip=" + ip;
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                query += "&location=" + location;
            }
            return new JsonResult(Zoonic.Interface.Ali.AliApiRequestClient.Request(host, query, Appcode, "", method));
        }
    }
}
