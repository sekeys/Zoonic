using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class WeatherEntity
    {
        public string City { get; set; }
        public string CityId { get; set; }
        public string CityCode { get; set; }
        public string Date { get; set; }
        public string Week { get; set; }
        public string Weather { get; set; }
        public string Temp { get; set; }
        public Temperature Temperature { get; set; }

        public int ImageStatus { get; set; }
        public DateTime Timepoint { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<WeatherFeatures> Features { get; set; }
        public WeatherAQI AQI { get; set; }
        public List<WeatherDaily> Daily { get; set; }
        public List<WeatherHourly> Hourly { get; set; }
    }
    public class Temperature
    {
        public int Avg { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public string Humidity { get; set; }
        public string Pressure { get; set; }
        public double Windspeed { get; set; }
        public string Windpower { get; set; }
        public string WindDirect { get; set; }
    }
    public class WeatherFeatures
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
    }
    public class WeatherAQI
    {
        public string IO3_8 { get; set; }

        public string SO2 { get; set; }
        public string SO2_24 { get; set; }

        public string NO2 { get; set; }
        public string NO2_24 { get; set; }


        public string CO { get; set; }
        public string CO_24 { get; set; }


        public string O3 { get; set; }
        public string O3_8 { get; set; }
        public string O3_24 { get; set; }


        public string PM10 { get; set; }
        public string PM10_24 { get; set; }
        public string PM2_5 { get; set; }
        public string PM2_5_24 { get; set; }

        public string ISO2 { get; set; }

        public string INO2 { get; set; }
        public string ICO { get; set; }
        public string IO3 { get; set; }
        public string IPM10 { get; set; }
        public string IPM2_5 { get; set; }
        public string AQI { get; set; }
        public string Psrimarypollutant { get; set; }
        public string Quality { get; set; }
        public WeatherAQIInfo Info
        {
            get;
            set;
        }
    }
    public class WeatherAQIInfo
    {
        public string Level { get; set; }
        public string Color { get; set; }
        public string Affect { get; set; }
        public string Measure { get; set; }
    }
    public class WeatherDaily
    {
        public string Date { get; set; }
        public string Week { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public WeatherDailyInfo Night { get; set; }
        public WeatherDailyInfo Day { get; set; }
    }
    public class WeatherHourly
    {
        public string Time { get; set; }
        public int TimeHour { get; set; }
        public int TimeMins { get; set; }
        public int Temperature { get; set; }
        public string Weather { get; set; }
        public int ImageStatus { get; set; }
    }
    public class WeatherDailyInfo
    {
        public int ImageStatus { get; set; }
        public string Weather { get; set; }
        public Temperature Temperature { get; set; }
        //public string WindDirect { get; set; }
        //public string WindPower { get; set; }

    }

}
