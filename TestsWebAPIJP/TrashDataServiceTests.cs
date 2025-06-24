using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;
using TrashBoard.Components.Pages.Admin;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class TrashDataServiceTests
    {
        private class FakeHolidayService : IHolidayService
        {
            public Task<HolidayData?> IsHolidayAsync(DateTime date) =>
            Task.FromResult<HolidayData?>(new HolidayData { LocalName = "Fake Holiday", Date = date });

            public Task<List<HolidayData>> GetHolidaysForYearAsync(int year) =>
                Task.FromResult(new List<HolidayData>());
        }

        private class FakeBredaEventService : IBredaEventService
        {
            public Task<BredaEvent?> HasBredaEventAsync(DateTime date) =>
                Task.FromResult<BredaEvent?>(new BredaEvent { Name = "Fake Event", StartDate = date, EndDate = date });

            public Task<List<BredaEvent>> GetBredaEventsAsync(int year) =>
                Task.FromResult(new List<BredaEvent>());
        }

        private class FakeWeatherService : IWeatherService
        {
            public Task<Dictionary<string, WeatherData>> GetWeatherDataForRangeAsync(DateTime from, DateTime to) =>
                Task.FromResult(new Dictionary<string, WeatherData>());

            public Task<WeatherData?> GetWeatherForTimestampAsync(DateTime timestamp)
            {
                throw new NotImplementedException();
            }
        }

        private class DummyDbContextFactory : IDbContextFactory<TrashboardDbContext>
        {
            private readonly DbContextOptions<TrashboardDbContext> _options;

            public DummyDbContextFactory(DbContextOptions<TrashboardDbContext> options)
            {
                _options = options;
            }

            public TrashboardDbContext CreateDbContext()
            {
                return new TrashboardDbContext(_options);
            }
        }

        [TestMethod]
        public async Task GetCount_Returns_Correct_Count()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TrashboardDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Setup context and seed
            await using (var seedContext = new TrashboardDbContext(options))
            {
                seedContext.TrashDetections.Add(new TrashDetection { Timestamp = DateTime.Now, DetectedObject = "Can" });
                await seedContext.SaveChangesAsync();
            }

            var service = new TrashDataService(new DummyDbContextFactory(options), new FakeHolidayService(), new FakeBredaEventService(), new FakeWeatherService());

            var count = await service.GetCount();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task GetAvailableTrashTypesAsync_Returns_Distinct_Types()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TrashboardDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await using (var seedContext = new TrashboardDbContext(options))
            {
                seedContext.TrashDetections.AddRange(
                    new TrashDetection { DetectedObject = "Bottle" },
                    new TrashDetection { DetectedObject = "Can" },
                    new TrashDetection { DetectedObject = "Bottle" });
                await seedContext.SaveChangesAsync();
            }

            var service = new TrashDataService(new DummyDbContextFactory(options), new FakeHolidayService(), new FakeBredaEventService(), new FakeWeatherService());

            var types = await service.GetAvailableTrashTypesAsync();
            CollectionAssert.AreEquivalent(new[] { "Bottle", "Can" }, types.ToList());
        }



        [TestMethod]
        public async Task GetFilteredAsync_Filters_By_TrashType()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TrashboardDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await using (var seedContext = new TrashboardDbContext(options))
            {
                seedContext.TrashDetections.AddRange(
                    new TrashDetection { DetectedObject = "Bottle", Timestamp = DateTime.Today },
                    new TrashDetection { DetectedObject = "Can", Timestamp = DateTime.Today });
                await seedContext.SaveChangesAsync();
            }

            var service = new TrashDataService(new DummyDbContextFactory(options), new FakeHolidayService(), new FakeBredaEventService(), new FakeWeatherService());

            var result = await service.GetFilteredAsync(null, null, new List<string> { "Bottle" }, null, null);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Bottle", result.First().DetectedObject);
        }
        [TestMethod]
        public async Task AddAsync_Adds_TrashDetection_With_Enriched_Data()
        {
            // Use same DB name for both service and verification
            var dbName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<TrashboardDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var factory = new DummyDbContextFactory(options);
            var service = new TrashDataService(factory, new FakeHolidayService(), new FakeBredaEventService(), new FakeWeatherService());

            var detection = new TrashDetection
            {
                Id = 1,
                Timestamp = DateTime.Now,
                DetectedObject = "Bottle"
            };

            // Act
            await service.AddAsync(detection);

            // Assert using a *new* context
            await using var verifyContext = new TrashboardDbContext(options);
            var saved = await verifyContext.TrashDetections.FirstOrDefaultAsync();

            Assert.IsNotNull(saved);
            Assert.AreEqual("Bottle", saved.DetectedObject);
            Assert.AreEqual("Fake Holiday", saved.HolidayName);
            Assert.AreEqual("Fake Event", saved.BredaEventName);
            Assert.IsTrue(saved.IsHoliday);
            Assert.IsTrue(saved.IsBredaEvent);
        }


    }
}
