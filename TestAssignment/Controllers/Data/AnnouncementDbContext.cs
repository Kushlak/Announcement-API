using Microsoft.EntityFrameworkCore;

namespace TestAssignment.Controllers.Data
{
    public class AnnouncementDbContext(DbContextOptions<AnnouncementDbContext> options) : DbContext(options)
    {
        public DbSet<Announcement> Announcements => Set<Announcement>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Announcement>().HasData(
                 new Announcement
                 {
                     Id = 1,
                     Description = "moving forward",
                     Title = "Meeting",
                     Publisher = "Sheila"
                 },
            new Announcement
            {
                Id = 2,
                Title = "Alliance",
                Publisher = "Anthony"
            }
            );
        }
    }
}
