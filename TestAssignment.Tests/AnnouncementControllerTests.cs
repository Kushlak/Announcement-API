using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAssignment;
using TestAssignment.Controllers;
using TestAssignment.Controllers.Data;
using Xunit;

namespace TestAssignment.Tests.Controllers
{
    public class AnnouncementControllerTests
    {
        [Fact]
        public async Task GetAllAnnouncements_Returns_Ok_With_List()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("GetAllAnnouncementsDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            context.Announcements.Add(new Announcement { Title = "Anna renting flat", Description = "Nice view", DateAdded = new DateTime(2025, 1, 15), Publisher = "Anna"});
            context.Announcements.Add(new Announcement { Title = "John selling bike", Description = "Good price", DateAdded = DateTime.UtcNow, Publisher = "Anna"});
            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var result = await controller.GetAllAnnouncements();

            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);

            var list = okResult.Value as List<Announcement>;
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetAnnouncementById_Returns_Ok_When_Found()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("GetByIdFoundDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            var maria = new Announcement { Title = "Maria looking for roommate", Description = "Quiet person", DateAdded = new DateTime(2025, 8, 12), Publisher = "Maria" };
            context.Announcements.Add(maria);
            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var result = await controller.GetAnnouncementById(maria.Id);

            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);

            var announcement = okResult.Value as Announcement;
            Assert.NotNull(announcement);
            Assert.Equal("Maria looking for roommate", announcement.Title);
        }

        [Fact]
        public async Task GetAnnouncementById_Returns_NotFound_When_Missing()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("GetByIdMissingDb")
                .Options;
            var context = new AnnouncementDbContext(options);
            var controller = new AnnouncementController(context);

            var result = await controller.GetAnnouncementById(999);

            var notFound = result.Result as NotFoundResult;
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task AddAnnouncment_Returns_Created_When_Valid()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("AddAnnouncementDb")
                .Options;
            var context = new AnnouncementDbContext(options);
            var controller = new AnnouncementController(context);

            var newAnnounce = new Announcement
            {
                Title = "Peter selling guitar",
                Description = "Almost new, good sound",
                Publisher = "Peter",
                DateAdded = new DateTime(2025, 1, 2)
            };

            var result = await controller.AddAnnouncment(newAnnounce);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.NotNull(createdResult);

            var body = createdResult.Value as Announcement;
            Assert.NotNull(body);
            Assert.Equal("Peter selling guitar", body.Title);

            Assert.Equal(201, createdResult.StatusCode);
            Assert.Single(context.Announcements);
        }

        [Fact]
        public async Task AddAnnouncment_Returns_BadRequest_When_Null()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("AddAnnouncementNullDb")
                .Options;
            var context = new AnnouncementDbContext(options);
            var controller = new AnnouncementController(context);

            var result = await controller.AddAnnouncment(null);

            var badRequest = result.Result as BadRequestResult;
            Assert.NotNull(badRequest);
        }

        [Fact]
        public async Task UpdateAnnouncement_Returns_NoContent_When_Exists()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("UpdateAnnouncementDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            var lauraOld = new Announcement { Title = "Laura selling car", Description = "Old but reliable", Publisher = "Laura", DateAdded = DateTime.UtcNow};
            context.Announcements.Add(lauraOld);
            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var lauraNew = new Announcement
            {
                Title = "Laura selling car fast",
                Description = "Lower price today",
                Publisher = "Laura",
                DateAdded = new DateTime(2025, 10, 23)
            };

            var result = await controller.UpdateAnnouncement(lauraOld.Id, lauraNew);

            var noContent = result as NoContentResult;
            Assert.NotNull(noContent);

            var entity = await context.Announcements.FindAsync(lauraOld.Id);
            Assert.NotNull(entity);
            Assert.Equal("Laura selling car fast", entity.Title);
            Assert.Equal(new DateTime(2025, 10, 23), entity.DateAdded);
        }

        [Fact]
        public async Task UpdateAnnouncement_Returns_NotFound_When_NotExists()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("UpdateAnnouncementMissingDb")
                .Options;
            var context = new AnnouncementDbContext(options);
            var controller = new AnnouncementController(context);

            var update = new Announcement { Title = "Paul buying books", Description = "Used ones", DateAdded = new DateTime(2025, 2, 3)};

            var result = await controller.UpdateAnnouncement(999, update);

            var notFound = result as NotFoundResult;
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task DeleteAnnouncement_Returns_NoContent_When_Exists()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("DeleteAnnouncementDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            var sophie = new Announcement { Title = "Sophie selling desk", Description = "Wooden desk", DateAdded = new DateTime(2025, 8, 4)};
            context.Announcements.Add(sophie);
            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var result = await controller.DeleteAnnouncement(sophie.Id);

            var noContent = result as NoContentResult;
            Assert.NotNull(noContent);
            Assert.Empty(context.Announcements);
        }

        [Fact]
        public async Task DeleteAnnouncement_Returns_NotFound_When_NotExists()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("DeleteAnnouncementMissingDb")
                .Options;
            var context = new AnnouncementDbContext(options);
            var controller = new AnnouncementController(context);

            var result = await controller.DeleteAnnouncement(123);

            var notFound = result as NotFoundResult;
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task GetAnnouncementsFiltered_WithoutQuery_Returns_Top3_ByDate()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("FilteredNoQueryDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            context.Announcements.AddRange(
                new Announcement { Title = "Nick giving away sofa", DateAdded = DateTime.UtcNow.AddDays(-3) },
                new Announcement { Title = "Tom offering help", DateAdded = DateTime.UtcNow.AddDays(-2) },
                new Announcement { Title = "Elena selling phone", DateAdded = DateTime.UtcNow.AddDays(-1) },
                new Announcement { Title = "Alan exists", DateAdded = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var result = await controller.GetAnnouncementsFiltered(null);

            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);

            var list = okResult.Value as List<Announcement>;
            Assert.NotNull(list);
            Assert.Equal(3, list.Count);
            Assert.Equal("Alan exists", list[0].Title);
            Assert.Equal("Elena selling phone", list[1].Title);
            Assert.Equal("Tom offering help", list[2].Title);
        }

        [Fact]
        public async Task GetAnnouncementsFiltered_WithQuery_Finds_By_Word()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase("FilteredWithQueryDb")
                .Options;
            var context = new AnnouncementDbContext(options);

            context.Announcements.AddRange(
                new Announcement { Title = "Anna selling cat bed", Description = "Soft and cozy", DateAdded = DateTime.UtcNow.AddDays(-1)},
                new Announcement { Title = "David selling dog leash", Description = "Strong", DateAdded = DateTime.UtcNow.AddDays(-2)},
                new Announcement { Title = "Emma giving cat food", Description = "Leftovers", DateAdded = DateTime.UtcNow.AddDays(-3)}
            );
            await context.SaveChangesAsync();

            var controller = new AnnouncementController(context);

            var result = await controller.GetAnnouncementsFiltered("cat");

            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);

            var list = okResult.Value as List<Announcement>;
            Assert.NotNull(list);

            foreach (var a in list)
            {
                var text = (a.Title + " " + a.Description).ToLower();
                Assert.Contains("cat", text);
            }
        }
    }
}
