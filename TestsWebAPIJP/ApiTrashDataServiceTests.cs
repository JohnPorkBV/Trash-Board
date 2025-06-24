using RichardSzalay.MockHttp;
using System.Globalization;
using System.Net;
using System.Text.Json;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class ApiTrashDataServiceTests
    {
        private static List<TrashDetectionApiModel> GetSampleApiData() => new()
        {
            new TrashDetectionApiModel
            {
                Label = "Bottle",
                Confidence = 0.9234,
                Timestamp = new DateTime(2024, 6, 24, 14, 0, 0),
                Temperature = 22.5,
                Humidity = 55.2f,
                Wind = 3.1
            },
            new TrashDetectionApiModel
            {
                Label = "Can",
                Confidence = 0.8123,
                Timestamp = new DateTime(2024, 6, 24, 15, 30, 0),
                Temperature = 23.2,
                Humidity = 60.1f,
                Wind = 2.9
            }
        };

        [TestMethod]
        public async Task GetAllAsync_Returns_Parsed_TrashDetections()
        {
            var mockData = GetSampleApiData();
            var json = JsonSerializer.Serialize(mockData);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost/api/trashdetect")
                    .Respond("application/json", json);

            var client = new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost") };
            var service = new ApiTrashDataService(client);

            var result = await service.GetAllAsync();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Bottle", result[0].DetectedObject);
            Assert.AreEqual("0.92", result[0].ConfidenceScore);
            Assert.AreEqual(14, result[0].Hour);
        }

        [TestMethod]
        public async Task GetSinceAsync_Returns_Filtered_By_Timestamp()
        {
            var mockData = GetSampleApiData(); // includes 14:00 and 15:30
            var json = JsonSerializer.Serialize(mockData);

            var mockHttp = new MockHttpMessageHandler();
            var queryDate = new DateTime(2024, 6, 24).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            mockHttp.When($"http://localhost/api/trashdetect/date?date={queryDate}")
                    .Respond("application/json", json);

            var client = new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost") };
            var service = new ApiTrashDataService(client);

            var since = new DateTime(2024, 6, 24, 15, 0, 0); // Should only return Can

            var result = await service.GetSinceAsync(since);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Can", result[0].DetectedObject);
        }

        [TestMethod]
        public async Task GetAllAsync_Throws_On_Error()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost/api/trashdetect")
                    .Respond(HttpStatusCode.InternalServerError);

            var client = new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost") };
            var service = new ApiTrashDataService(client);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await service.GetAllAsync();
            });
        }
    }
}
