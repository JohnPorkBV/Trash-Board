using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class BredaEventServiceTests
    {
        private TrashboardDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<TrashboardDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Ensures isolation per test
                .Options;
            return new TrashboardDbContext(options);
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Event_On_Single_Day()
        {
            using var context = CreateInMemoryDbContext();

            context.BredaEvents.Add(new BredaEvent
            {
                Id = 1,
                StartDate = new DateTime(2024, 6, 24),
                EndDate = null,
                Name = "Eendaags Event",
                Location = "Breda",
                Description = "Test"
            });
            await context.SaveChangesAsync();

            var service = new BredaEventService(context, new HttpClient());
            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 24));

            Assert.IsNotNull(result);
            Assert.AreEqual("Eendaags Event", result!.Name);
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Event_On_MultiDay_Event()
        {
            using var context = CreateInMemoryDbContext();

            context.BredaEvents.Add(new BredaEvent
            {
                Id = 2,
                StartDate = new DateTime(2024, 6, 20),
                EndDate = new DateTime(2024, 6, 25),
                Name = "Meerdaags Event",
                Location = "Breda",
                Description = "Test"
            });
            await context.SaveChangesAsync();

            var service = new BredaEventService(context, new HttpClient());
            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 22));

            Assert.IsNotNull(result);
            Assert.AreEqual("Meerdaags Event", result!.Name);
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Null_When_No_Event()
        {
            using var context = CreateInMemoryDbContext();

            // No events added
            var service = new BredaEventService(context, new HttpClient());
            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 24));

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetBredaEventsAsync_Returns_Events_For_Year()
        {
            using var context = CreateInMemoryDbContext();

            context.BredaEvents.AddRange(
                new BredaEvent
                {
                    Id = 1,
                    StartDate = new DateTime(2024, 1, 15),
                    EndDate = null,
                    Name = "2024 Event",
                    Location = "Breda",
                    Description = "Jaarlijks"
                },
                new BredaEvent
                {
                    Id = 2,
                    StartDate = new DateTime(2023, 12, 31),
                    EndDate = new DateTime(2024, 1, 2),
                    Name = "Spanning Event",
                    Location = "Breda",
                    Description = "Meerdaags"
                },
                new BredaEvent
                {
                    Id = 3,
                    StartDate = new DateTime(2025, 5, 10),
                    EndDate = null,
                    Name = "2025 Event",
                    Location = "Breda",
                    Description = "Volgend jaar"
                }
            );
            await context.SaveChangesAsync();

            var service = new BredaEventService(context, new HttpClient());
            var result = await service.GetBredaEventsAsync(2024);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "2024 Event"));
            Assert.IsTrue(result.Any(e => e.Name == "Spanning Event"));
        }

        [TestMethod]
        public async Task GetBredaEventsAsync_Returns_Empty_When_No_Events()
        {
            using var context = CreateInMemoryDbContext();

            var service = new BredaEventService(context, new HttpClient());
            var result = await service.GetBredaEventsAsync(2024);

            Assert.AreEqual(0, result.Count);
        }
    }
}
