using BlazorClient.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorClient
{
    public class WeatherAPIService
    {
        private readonly HttpClient _httpClient;

        public WeatherAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetAll()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(await _httpClient.GetStringAsync("/WeatherForecast/"));
        }

    }
}
