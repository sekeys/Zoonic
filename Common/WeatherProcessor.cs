using Common.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoonic.Entities;
using Zoonic.Extensions;
using Zoonic.Web;

namespace Common
{
    public class WeatherProcessor : ProcessorBase
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
            var str = Zoonic.Interface.Ali.AliApiRequestClient.Request(host, query, Appcode, "", method);
            return new JsonResult(Convert(str));
        }
        protected Zoonic.Entities.ApiEntityResult<WeatherEntity> Convert(string str)
        {
            ApiEntityResult<WeatherEntity> result = new ApiEntityResult<WeatherEntity>();

            JObject jObject = JObject.Parse(str);
            result.ApiInterface = "weather.v.1";
            result.Expiration = -1;
            var jStatus = jObject.SelectToken("status");
            var status = jStatus.Value<int>();
            try
            {
                if (status == 0)
                {
                    var jResult = jObject.SelectToken("result");
                    var entity = new WeatherEntity();
                    entity.City = jResult.SelectToken("city")?.Value<string>();
                    entity.CityCode = jResult.SelectToken("citycode")?.Value<string>();
                    entity.CityId = jResult.SelectToken("cityid")?.Value<string>();
                    entity.Date = jResult.SelectToken("date")?.Value<string>();
                    entity.Week = jResult.SelectToken("week")?.Value<string>();
                    entity.Weather = jResult.SelectToken("weather")?.Value<string>();
                    entity.Temperature = new Temperature();
                    entity.Temperature.Avg = jResult.SelectToken("temp").Value<int>();
                    entity.Temperature.High = jResult.SelectToken("temphigh").Value<int>();
                    entity.Temperature.Low = jResult.SelectToken("templow").Value<int>();
                    entity.ImageStatus = jResult.SelectToken("img").Value<int>();
                    entity.Temperature.Windpower = jResult.SelectToken("windpower").Value<string>();
                    entity.Temperature.Windspeed = jResult.SelectToken("windspeed").Value<double>();
                    entity.Temperature.WindDirect = jResult.SelectToken("winddirect").Value<string>();
                    entity.Temperature.Humidity = jResult.SelectToken("humidity").Value<string>();
                    entity.Temperature.Pressure = jResult.SelectToken("pressure").Value<string>();
                    entity.UpdateDate = jResult.SelectToken("updatetime").Value<DateTime>();
                    entity.Features = new List<WeatherFeatures>();
                    var jIndex = jResult.SelectToken("index");
                    foreach (var index in jIndex.Children())
                    {
                        var features = new WeatherFeatures();
                        features.Name = index.SelectToken("iname").Value<string>();
                        features.Value = index.SelectToken("ivalue").Value<string>();
                        features.Detail = index.SelectToken("detail").Value<string>();
                        entity.Features.Add(features);
                    }
                    entity.AQI = new WeatherAQI();
                    var jAqi = jResult.SelectToken("aqi");
                    entity.AQI.SO2 = jAqi.SelectToken("so2").Value<string>();
                    entity.AQI.SO2_24 = jAqi.SelectToken("so224").Value<string>();
                    entity.AQI.NO2 = jAqi.SelectToken("no2").Value<string>();
                    entity.AQI.NO2_24 = jAqi.SelectToken("no224").Value<string>();
                    entity.AQI.CO = jAqi.SelectToken("co").Value<string>();
                    entity.AQI.CO_24 = jAqi.SelectToken("co24").Value<string>();
                    entity.AQI.O3 = jAqi.SelectToken("o3").Value<string>();
                    entity.AQI.O3_8 = jAqi.SelectToken("o38").Value<string>();
                    entity.AQI.O3_24 = jAqi.SelectToken("o324").Value<string>();
                    entity.AQI.PM10 = jAqi.SelectToken("pm10").Value<string>();
                    entity.AQI.PM10_24 = jAqi.SelectToken("pm1024").Value<string>();
                    entity.AQI.PM2_5 = jAqi.SelectToken("pm2_5").Value<string>();
                    entity.AQI.PM2_5_24 = jAqi.SelectToken("pm2_524").Value<string>();
                    entity.AQI.ISO2 = jAqi.SelectToken("iso2").Value<string>();
                    entity.AQI.INO2 = jAqi.SelectToken("ino2").Value<string>();
                    entity.AQI.ICO = jAqi.SelectToken("ico").Value<string>();
                    entity.AQI.IO3 = jAqi.SelectToken("io3").Value<string>();
                    entity.AQI.IO3_8 = jAqi.SelectToken("io38").Value<string>();
                    entity.AQI.IPM10 = jAqi.SelectToken("ipm10").Value<string>();
                    entity.AQI.IPM2_5 = jAqi.SelectToken("ipm2_5").Value<string>();
                    entity.AQI.AQI = jAqi.SelectToken("aqi").Value<string>();
                    entity.AQI.Psrimarypollutant = jAqi.SelectToken("primarypollutant").Value<string>();
                    entity.AQI.Quality = jAqi.SelectToken("quality").Value<string>();
                    entity.Timepoint = jAqi.SelectToken("timepoint").Value<DateTime>();
                    entity.AQI.Info = new WeatherAQIInfo();
                    var jaqiInfo = jAqi.SelectToken("aqiinfo");
                    entity.AQI.Info.Level = jaqiInfo.SelectToken("level").Value<string>();
                    entity.AQI.Info.Color = jaqiInfo.SelectToken("color").Value<string>();
                    entity.AQI.Info.Affect = jaqiInfo.SelectToken("affect").Value<string>();
                    entity.AQI.Info.Measure = jaqiInfo.SelectToken("measure").Value<string>();
                    var jDaily = jResult.SelectToken("daily");
                    entity.Daily = new List<WeatherDaily>();
                    foreach (var item in jDaily.Children())
                    {
                        var daily = new WeatherDaily();
                        daily.Date = item.SelectToken("date").Value<string>();
                        daily.Week = item.SelectToken("week").Value<string>();
                        daily.Sunrise = item.SelectToken("sunrise").Value<string>();
                        daily.Sunset = item.SelectToken("sunset").Value<string>();
                        daily.Night = new WeatherDailyInfo();
                        daily.Night.Temperature = new Temperature();
                        var jNight = item.SelectToken("night");

                        daily.Night.Temperature.Low = jNight.SelectToken("templow").Value<int>();
                        daily.Night.Temperature.WindDirect = jNight.SelectToken("winddirect").Value<string>();
                        daily.Night.Temperature.Windpower = jNight.SelectToken("windpower").Value<string>();
                        daily.Night.Weather = jNight.SelectToken("weather").Value<string>();
                        daily.Night.ImageStatus = jNight.SelectToken("img").Value<int>();



                        daily.Day = new WeatherDailyInfo();
                        daily.Day.Temperature = new Temperature();
                        var jDay = item.SelectToken("day");

                        daily.Day.Temperature.High = jDay.SelectToken("temphigh").Value<int>();
                        daily.Day.Temperature.WindDirect = jDay.SelectToken("winddirect").Value<string>();
                        daily.Day.Temperature.Windpower = jDay.SelectToken("windpower").Value<string>();
                        daily.Day.Weather = jDay.SelectToken("weather").Value<string>();
                        daily.Day.ImageStatus = jDay.SelectToken("img").Value<int>();
                        entity.Daily.Add(daily);
                    }
                    entity.Hourly = new List<WeatherHourly>();
                    var jHourly = jResult.SelectToken("hourly");
                    foreach (var item in jHourly.Children())
                    {
                        var hour = new WeatherHourly();
                        hour.Time = item.SelectToken("time").Value<string>();
                        var split = hour.Time.Split(':');
                        hour.TimeHour = System.Convert.ToInt32(split[0]);
                        hour.TimeMins = System.Convert.ToInt32(split[1]);
                        hour.ImageStatus = item.SelectToken("img").Value<int>();
                        hour.Temperature = item.SelectToken("temp").Value<int>();
                        hour.Weather = item.SelectToken("weather").Value<string>();

                        entity.Hourly.Add(hour);
                    }
                    result.StatusCode = 200;
                    result.Data = entity;
                }
                else
                {
                    result.Result = false;
                    result.StatusCode = status;
                    switch (result.StatusCode)
                    {
                        case 201:
                            result.Tag = "城市和城市ID和城市代号都为空";
                            break;
                        case 202:
                            result.Tag = "城市不存在";
                            break;
                        case 203:
                            result.Tag = "此城市没有天气信息";
                            break;
                        case 210:
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
