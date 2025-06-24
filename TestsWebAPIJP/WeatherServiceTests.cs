using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class WeatherServiceTests
    {
        private const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";
        private const string ArchiveUrl = "https://archive-api.open-meteo.com/v1/archive";

        private MockHttpMessageHandler SetupMockHandler(string json)
        {
            var mockHttp = new MockHttpMessageHandler();

            // Forecast endpoint
            mockHttp.When($"{ForecastUrl}*")
                .Respond("application/json", json);

            // Archive endpoint
            mockHttp.When($"{ArchiveUrl}*")
                .Respond("application/json", json);

            return mockHttp;
        }

        [TestMethod]
        public async Task GetWeatherDataForRangeAsync_Returns_WeatherData()
        {
            var json = @"{
                ""hourly"": {
                    ""time"": [""2024-06-24T14:00""],
                    ""temperature_2m"": [22.5],
                    ""precipitation"": [0.1],
                    ""wind_speed_10m"": [3.2],
                    ""relative_humidity_2m"": [55]
                }
            }";

            var mockHttp = SetupMockHandler(json);
            var service = new WeatherService(new HttpClient(mockHttp));

            var result = await service.GetWeatherDataForRangeAsync(new DateTime(2024, 6, 24), new DateTime(2024, 6, 24));

            Assert.IsTrue(result.ContainsKey("2024-06-24T14:00"));
            var data = result["2024-06-24T14:00"];
            Assert.AreEqual(22.5f, data.Temp);
            Assert.AreEqual(0.1f, data.Precipitation);
            Assert.AreEqual(3.2f, data.Windforce);
            Assert.AreEqual(55, data.Humidity);
        }

        [TestMethod]
        public async Task GetWeatherDataForRangeAsync_Returns_Empty_When_No_Hourly_Data()
        {
            var json = @"{ }";
            var mockHttp = SetupMockHandler(json);
            var service = new WeatherService(new HttpClient(mockHttp));

            var result = await service.GetWeatherDataForRangeAsync(new DateTime(2024, 6, 24), new DateTime(2024, 6, 24));

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetWeatherDataForRangeAsync_Handles_Partial_Data()
        {
            var json = @"{
                ""hourly"": {
                    ""time"": [""2024-06-24T14:00""],
                    ""temperature_2m"": [20.0]
                }
            }";

            var mockHttp = SetupMockHandler(json);
            var service = new WeatherService(new HttpClient(mockHttp));

            var result = await service.GetWeatherDataForRangeAsync(new DateTime(2024, 6, 24), new DateTime(2024, 6, 24));

            Assert.IsTrue(result.ContainsKey("2024-06-24T14:00"));
            var data = result["2024-06-24T14:00"];

            Assert.AreEqual(20.0f, data.Temp);
            Assert.AreEqual(0.0f, data.Precipitation);  // missing = default
            Assert.AreEqual(0.0f, data.Windforce);      // missing = default
            Assert.AreEqual(0, data.Humidity);          // missing = default
        }
    }
}
