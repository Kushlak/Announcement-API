using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAssignment;
using TestAssignment.Controllers;
using TestAssignment.Controllers.Data;

public class AnnouncementController_Tests
{
    static int Unix(DateTime dtUtc) => (int)new DateTimeOffset(dtUtc).ToUnixTimeSeconds();
    private static AnnouncementDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<AnnouncementDbContext>()
            .UseInMemoryDatabase(name).Options);

    private static async Task SeedAsync(AnnouncementDbContext db)
    {
        var now = DateTime.UtcNow;
        db.Announcements.AddRange(
            new Announcement { Id = 1, Title = "Cats", Description = "Adopt a cat", Publisher = "A", DateAdded = Unix(now.AddDays(-1)) },
            new Announcement { Id = 2, Title = "Dogs", Description = "Adopt a dog", Publisher = "B", DateAdded = Unix(now.AddDays(-2)) },
            new Announcement { Id = 3, Title = "Parrots", Description = "Green parrot", Publisher = "C", DateAdded = Unix(now.AddDays(-3)) },
            new Announcement { Id = 4, Title = "Kittens", Description = "Two kittens", Publisher = "D", DateAdded = Unix(now.AddDays(-4)) }
        );
        await db.SaveChangesAsync();

    }

    [Fact]
    public async Task GetAllAnnouncements()
    {
        using var db = Ctx(nameof(GetAllAnnouncements));
        await SeedAsync(db);
        var c = new AnnouncementController(db);

        var result = await c.GetAllAnnouncements();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<Announcement>>(ok.Value);
        Assert.Equal(4, list.Count);
    }

    [Fact]
    public async Task GetById_Found_And_NotFound()
    {
        using var db = Ctx(nameof(GetById_Found_And_NotFound));
        await SeedAsync(db);
        var c = new AnnouncementController(db);

        var found = await c.GetAnnouncementById(2);
        var missing = await c.GetAnnouncementById(999);

        var ok = Assert.IsType<OkObjectResult>(found.Result);
        Assert.Equal(2, Assert.IsType<Announcement>(ok.Value).Id);
        Assert.IsType<NotFoundResult>(missing.Result);
    }

    [Fact]
    public async Task Add()
    {
        using var db = Ctx(nameof(Add));
        var c = new AnnouncementController(db);

        var now = DateTime.UtcNow;
        var dto = new Announcement
        {
            Id = 100,
            Title = "New",
            Description = "Desc",
            Publisher = "Admin",
            DateAdded = Unix(now)   // ✅ кладемо Unix-секунди
        };

        var res = await c.AddAnnouncment(dto);

        var created = Assert.IsType<CreatedAtActionResult>(res.Result);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(nameof(AnnouncementController.GetAnnouncementById), created.ActionName);
        Assert.Equal(100, created.RouteValues!["id"]);

        var fromDb = await db.Announcements.FindAsync(100);
        Assert.NotNull(fromDb);
        Assert.Equal(Unix(now), fromDb!.DateAdded); // ✅ перевіряємо збережену дату
    }


    [Fact]
    public async Task Update_And_ChangesSaved()
    {
        using var db = Ctx(nameof(Update_And_ChangesSaved));
        await SeedAsync(db);
        var c = new AnnouncementController(db);

        var when = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        var update = new Announcement
        {
            Title = "UPDATED",
            Description = "UPDATED DESC",
            Publisher = "X",
            DateAdded = Unix(when)  // ✅ Unix-секунди
        };

        var res = await c.UpdateAnnouncement(1, update);

        Assert.IsType<NoContentResult>(res);

        var a = await db.Announcements.FindAsync(1);
        Assert.NotNull(a);
        Assert.Equal("UPDATED", a!.Title);
        Assert.Equal("UPDATED DESC", a.Description);
        Assert.Equal("X", a.Publisher);
        Assert.Equal(Unix(when), a.DateAdded); // ✅ звіряємо Unix
    }


    [Fact]
    public async Task Delete()
    {
        using var db = Ctx(nameof(Delete));
        await SeedAsync(db);
        var c = new AnnouncementController(db);

        var res = await c.DeleteAnnouncement(2);

        Assert.IsType<NoContentResult>(res);
        Assert.Null(await db.Announcements.FindAsync(2));
    }

    [Fact]
    public async Task Filtered_Top3_By_DateDesc()
    {
        using var db = Ctx(nameof(Filtered_Top3_By_DateDesc));
        await SeedAsync(db);
        var c = new AnnouncementController(db);

        var res = await c.GetAnnouncementsFiltered(null);

        var ok = Assert.IsType<OkObjectResult>(res.Result);
        var list = Assert.IsAssignableFrom<List<Announcement>>(ok.Value);
        Assert.Equal(new[] { 1, 2, 3 }, list.Select(x => x.Id).ToArray());
    }
}
