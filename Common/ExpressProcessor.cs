using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoonic.Common.Entity;
using Zoonic.Entities;
using Zoonic.Web;

namespace Common
{
    public class ExpressProcessor : ProcessorBase
    {
        private const string host = "http://jisukdcx.market.alicloudapi.com/express/query";
        private const string method = "GET";
        private const string Appcode = "49cc636b374744ea8dfdd81dc102dc62";

        public override async Task<ProcessorResult> Process()
        {
            string no = Request.Query["no"].ToString();
            string type = Request.Query["type"].ToString();
            string query = "nocache=" + DateTime.Now.Ticks;
            if (!string.IsNullOrWhiteSpace(no))
            {
                query += "&number=" + no;
            }
            if (string.IsNullOrWhiteSpace(type))
            {
                type = "auto";
            }
            query += "&type=" + type;
            var str = Zoonic.Interface.Ali.AliApiRequestClient.Request(host, query, Appcode, "", method);
            return new JsonResult(Convert(str));
        }
        public ApiEntityResult<Zoonic.Common.Entity.ExpressEntity> Convert(string str)
        {

            ApiEntityResult<ExpressEntity> result = new ApiEntityResult<ExpressEntity>();

            JObject jObject = JObject.Parse(str);
            result.ApiInterface = "express.v.1";
            result.Expiration = -1;
            var jStatus = jObject.SelectToken("status");
            var status = jStatus.Value<int>();
            try
            {
                if (status == 0)
                {
                    var jResult = jObject.SelectToken("result");
                    var entity = new ExpressEntity();
                    entity.Number = jResult.SelectToken("number").Value<string>();
                    entity.DeliveryStatus = jResult.SelectToken("deliverystatus").Value<int>();
                    entity.Type = jResult.SelectToken("type")?.Value<string>();
                    entity.Issign = jResult.SelectToken("issign").Value<int>();
                    entity.List = new List<ExpressItem>();
                    foreach (var item in jResult.SelectToken("list").Children())
                    {
                        var express = new ExpressItem();
                        express.Message = item.SelectToken("status").Value<string>();
                        express.Time = item.SelectToken("time").Value<DateTime>();
                        entity.List.Add(express);
                    }
                    result.Data = entity;
                    result.Result = true;
                    result.StatusCode = 200;
                }
                else
                {
                    result.Result = false;
                    result.StatusCode = status;
                    switch (result.StatusCode)
                    {
                        case 201:
                            result.Tag = "快递单号为空";
                            break;
                        case 202:
                            result.Tag = "快递公司为空";
                            break;
                        case 203:
                            result.Tag = "快递公司不存在";
                            break;
                        case 204:
                            result.Tag = "快递公司识别失败";
                            break;
                        case 205:
                            result.Tag = "没有信息";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.StatusCode = 500;
                result.Tag = ex.Message;
                result.Result = false;
                result.Token = ex.StackTrace;
            }
            return result;
        }
    }
}

