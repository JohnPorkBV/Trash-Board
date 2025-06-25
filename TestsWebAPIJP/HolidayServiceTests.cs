using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class HolidayServiceTests
    {
        private const string ApiBase = "https://date.nager.at";

        private static List<HolidayData> GetSampleHolidays() => new()
        {
            new HolidayData { Date = new DateTime(2024, 4, 27), LocalName = "Koningsdag" },
            new HolidayData { Date = new DateTime(2024, 12, 25), LocalName = "Kerstmis" }
        };

        [TestMethod]
        public async Task GetHolidaysForYearAsync_Returns_Holidays_From_Api()
        {
            // Arrange
            var holidays = GetSampleHolidays();
            var json = JsonSerializer.Serialize(holidays);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{ApiBase}/api/v3/PublicHolidays/2024/NL")
                    .Respond("application/json", json);

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(ApiBase)
            };

            var service = new HolidayService(client);

            // Act
            var result = await service.GetHolidaysForYearAsync(2024);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Koningsdag", result[0].LocalName);
        }

        [TestMethod]
        public async Task IsHolidayAsync_Returns_Correct_Holiday_If_Match()
        {
            var holidays = GetSampleHolidays();
            var json = JsonSerializer.Serialize(holidays);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{ApiBase}/api/v3/PublicHolidays/2024/NL")
                    .Respond("application/json", json);

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(ApiBase)
            };

            var service = new HolidayService(client);

            var date = new DateTime(2024, 4, 27);
            var result = await service.IsHolidayAsync(date);

            Assert.IsNotNull(result);
            Assert.AreEqual("Koningsdag", result!.LocalName);
        }

        [TestMethod]
        public async Task IsHolidayAsync_Returns_Null_When_Not_A_Holiday()
        {
            var holidays = GetSampleHolidays(); // Koningsdag and Kerstmis only
            var json = JsonSerializer.Serialize(holidays);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{ApiBase}/api/v3/PublicHolidays/2024/NL")
                    .Respond("application/json", json);

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(ApiBase)
            };

            var service = new HolidayService(client);

            var nonHoliday = new DateTime(2024, 6, 1);
            var result = await service.IsHolidayAsync(nonHoliday);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetHolidaysForYearAsync_Returns_Empty_On_Failure()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{ApiBase}/api/v3/PublicHolidays/2024/NL")
                    .Respond(HttpStatusCode.InternalServerError);

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(ApiBase)
            };

            var service = new HolidayService(client);

            var result = await service.GetHolidaysForYearAsync(2024);

            Assert.AreEqual(0, result.Count);
        }
    }
}

