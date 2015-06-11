using HydrationMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace HydrationMonitoring.Networking
{
    public class WeatherApiConnection
    {
        public static string ApiKey = "92f61886caf31cd6";
        public static string ApiUrl = "http://api.wunderground.com/api/{0}/conditions/q/{1},{2}.json";

        public static double LastTemperature = -1000;

        /// <summary>
        /// Gets weather information from the wunderground weather API and returns the current
        /// temperature in degrees
        /// </summary>
        /// <param name="coordinate">The position where the weather should be fetched</param>
        /// <returns></returns>
        public static async Task<double> GetTemperatureForPosition(Geocoordinate coordinate)
        {
            var uri = string.Format(ApiUrl, ApiKey, coordinate.Latitude, coordinate.Longitude);
            var hrm = new HttpRequestMessage(HttpMethod.Get, uri);
            hrm.Headers.Add("Key", ApiKey);

            HttpClient client = new HttpClient();
            var resp = await client.SendAsync(hrm);

            if (resp.StatusCode != HttpStatusCode.OK) return -1000;

            try
            {
                var stringResp = await (resp.Content as StreamContent).ReadAsStringAsync();
                var weatherData = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherConditions>(stringResp);
                LastTemperature = weatherData.current_observation.temp_c;
                return weatherData.current_observation.temp_c;
            }
            catch (Exception ex)
            {
                return -1000;
            }
            
        }

    }
}
