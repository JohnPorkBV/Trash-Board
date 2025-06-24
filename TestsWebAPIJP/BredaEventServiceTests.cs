using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;
using Microsoft.EntityFrameworkCore;

namespace TrashBoard.Tests.Services
{
    [TestClass]
    public class BredaEventServiceTests
    {
        private Mock<DbSet<BredaEvent>> CreateMockDbSet(List<BredaEvent> data)
        {
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<BredaEvent>>();
            mockSet.As<IQueryable<BredaEvent>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<BredaEvent>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<BredaEvent>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<BredaEvent>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Event_On_Single_Day()
        {
            // Arrange
            var events = new List<BredaEvent>
            {
                new BredaEvent
                {
                    Id = 1,
                    StartDate = new DateTime(2024, 6, 24),
                    EndDate = null,
                    Name = "Eendaags Event",
                    Location = "Breda",
                    Description = "Test"
                }
            };

            var mockSet = CreateMockDbSet(events);

            var mockContext = new Mock<TrashboardDbContext>();
            mockContext.Setup(c => c.BredaEvents).Returns(mockSet.Object);

            var service = new BredaEventService(mockContext.Object, new HttpClient());

            // Act
            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 24));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Eendaags Event", result!.Name);
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Event_On_MultiDay_Event()
        {
            var events = new List<BredaEvent>
            {
                new BredaEvent
                {
                    Id = 2,
                    StartDate = new DateTime(2024, 6, 20),
                    EndDate = new DateTime(2024, 6, 25),
                    Name = "Meerdaags Event",
                    Location = "Breda",
                    Description = "Test"
                }
            };

            var mockSet = CreateMockDbSet(events);

            var mockContext = new Mock<TrashboardDbContext>();
            mockContext.Setup(c => c.BredaEvents).Returns(mockSet.Object);

            var service = new BredaEventService(mockContext.Object, new HttpClient());

            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 22));

            Assert.IsNotNull(result);
            Assert.AreEqual("Meerdaags Event", result!.Name);
        }

        [TestMethod]
        public async Task HasBredaEventAsync_Returns_Null_When_No_Event()
        {
            var events = new List<BredaEvent>();

            var mockSet = CreateMockDbSet(events);

            var mockContext = new Mock<TrashboardDbContext>();
            mockContext.Setup(c => c.BredaEvents).Returns(mockSet.Object);

            var service = new BredaEventService(mockContext.Object, new HttpClient());

            var result = await service.HasBredaEventAsync(new DateTime(2024, 6, 24));

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetBredaEventsAsync_Returns_Events_For_Year()
        {
            var events = new List<BredaEvent>
            {
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
            };

            var mockSet = CreateMockDbSet(events);

            var mockContext = new Mock<TrashboardDbContext>();
            mockContext.Setup(c => c.BredaEvents).Returns(mockSet.Object);

            var service = new BredaEventService(mockContext.Object, new HttpClient());

            var result = await service.GetBredaEventsAsync(2024);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "2024 Event"));
            Assert.IsTrue(result.Any(e => e.Name == "Spanning Event"));
        }

        [TestMethod]
        public async Task GetBredaEventsAsync_Returns_Empty_When_No_Events()
        {
            var events = new List<BredaEvent>();

            var mockSet = CreateMockDbSet(events);

            var mockContext = new Mock<TrashboardDbContext>();
            mockContext.Setup(c => c.BredaEvents).Returns(mockSet.Object);

            var service = new BredaEventService(mockContext.Object, new HttpClient());

            var result = await service.GetBredaEventsAsync(2024);

            Assert.AreEqual(0, result.Count);
        }
    }
}
