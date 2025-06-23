using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TestsWebAPIJP
{
    [TestClass]
    public class TrashDataServiceTests
    {
        private Mock<IDbContextFactory<TrashboardDbContext>> _contextFactoryMock;
        private Mock<IHolidayService> _holidayServiceMock;
        private Mock<IBredaEventService> _bredaEventServiceMock;
        private Mock<IWeatherService> _weatherServiceMock;
        private TrashDataService _service;

        [TestInitialize]
        public void Setup()
        {
            _contextFactoryMock = new Mock<IDbContextFactory<TrashboardDbContext>>();
            _holidayServiceMock = new Mock<IHolidayService>();
            _bredaEventServiceMock = new Mock<IBredaEventService>();
            _weatherServiceMock = new Mock<IWeatherService>();
            _service = new TrashDataService(
                _contextFactoryMock.Object,
                _holidayServiceMock.Object,
                _bredaEventServiceMock.Object,
                _weatherServiceMock.Object);
        }

        [TestMethod]
        public async Task AddAsync_SetsHolidayAndEvent()
        {
            var detection = new TrashDetection { Timestamp = DateTime.Now };
            _holidayServiceMock.Setup(h => h.IsHolidayAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new HolidayData { LocalName = "TestHoliday" });
            _bredaEventServiceMock.Setup(b => b.HasBredaEventAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new BredaEvent { Name = "TestEvent" });

            var dbSetMock = new Mock<DbSet<TrashDetection>>();
            var dbContextMock = new Mock<TrashboardDbContext>();
            dbContextMock.Setup(c => c.TrashDetections).Returns(dbSetMock.Object);
            dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _contextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            await _service.AddAsync(detection);

            Assert.IsTrue(detection.IsHoliday);
            Assert.AreEqual("TestHoliday", detection.HolidayName);
            Assert.IsTrue(detection.IsBredaEvent);
            Assert.AreEqual("TestEvent", detection.BredaEventName);
            dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsAll()
        {
            var data = new List<TrashDetection> { new TrashDetection { Id = 1 }, new TrashDetection { Id = 2 } };
            var dbSetMock = new Mock<DbSet<TrashDetection>>();
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator());
            dbSetMock.Setup(m => m.AsNoTracking()).Returns(dbSetMock.Object);

            var dbContextMock = new Mock<TrashboardDbContext>();
            dbContextMock.Setup(c => c.TrashDetections).Returns(dbSetMock.Object);
            _contextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var result = await _service.GetAllAsync();
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetFilteredAsync_FiltersCorrectly()
        {
            var now = DateTime.Now;
            var data = new List<TrashDetection>
            {
                new TrashDetection { Id = 1, Timestamp = now, DetectedObject = "A", IsHoliday = true, IsBredaEvent = false },
                new TrashDetection { Id = 2, Timestamp = now.AddDays(-1), DetectedObject = "B", IsHoliday = false, IsBredaEvent = true }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<TrashDetection>>();
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            dbSetMock.Setup(m => m.AsNoTracking()).Returns(dbSetMock.Object);

            var dbContextMock = new Mock<TrashboardDbContext>();
            dbContextMock.Setup(c => c.TrashDetections).Returns(dbSetMock.Object);
            _contextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var result = await _service.GetFilteredAsync(now.AddDays(-2), now.AddDays(1), new List<string> { "A" }, true, false);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("A", result.First().DetectedObject);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsDetection()
        {
            var detection = new TrashDetection { Id = 42 };
            var dbSetMock = new Mock<DbSet<TrashDetection>>();
            dbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(detection);

            var dbContextMock = new Mock<TrashboardDbContext>();
            dbContextMock.Setup(c => c.TrashDetections).Returns(dbSetMock.Object);
            _contextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var result = await _service.GetByIdAsync(42);
            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.Id);
        }

        [TestMethod]
        public async Task GetAvailableTrashTypesAsync_ReturnsDistinctTypes()
        {
            var data = new List<TrashDetection>
            {
                new TrashDetection { DetectedObject = "A" },
                new TrashDetection { DetectedObject = "B" },
                new TrashDetection { DetectedObject = "A" }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<TrashDetection>>();
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<TrashDetection>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var dbContextMock = new Mock<TrashboardDbContext>();
            dbContextMock.Setup(c => c.TrashDetections).Returns(dbSetMock.Object);
            _contextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var result = await _service.GetAvailableTrashTypesAsync();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, result.ToList());
        }
    }
}