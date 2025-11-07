using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAssignment.Controllers.Data;

namespace TestAssignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController(AnnouncementDbContext context): ControllerBase
    {
        private readonly AnnouncementDbContext _context = context;
        
        [HttpGet("GetAllAnnouncements")]
        public async Task<ActionResult<List<Announcement>>> GetAllAnnouncements()
        {
            return Ok(await _context.Announcements.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Announcement>> GetAnnouncementById(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement is null)
                return NotFound();

            return Ok(announcement);
        }

        [HttpPost("add Announcement")]
        public async Task<ActionResult<Announcement>> AddAnnouncment(Announcement newAnnounce)
        {
            if (newAnnounce is null)
                return BadRequest();

            newAnnounce.DateAdded = DateTime.UtcNow;

            _context.Announcements.Add(newAnnounce);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnnouncementById), new { id = newAnnounce.Id }, newAnnounce);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(int id, Announcement updateAnnouncement)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement is null)
                return NotFound();

            announcement.Title = updateAnnouncement.Title;
            announcement.Description = updateAnnouncement.Description;
            announcement.DateAdded = updateAnnouncement.DateAdded;
            announcement.Publisher = updateAnnouncement.Publisher;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement is null)
                return NotFound();

            _context.Announcements.Remove(announcement);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("filtered")]
        public async Task<ActionResult<List<Announcement>>> GetAnnouncementsFiltered([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(await _context.Announcements
                    .OrderByDescending(a => a.DateAdded)
                    .Take(3)
                    .ToListAsync());

            var pattern = "%" + q.Replace(" ", "%") + "%";

            var list = await _context.Announcements
                .Where(a =>
                    EF.Functions.Like(a.Title.ToLower(), pattern.ToLower()) ||
                    EF.Functions.Like(a.Description.ToLower(), pattern.ToLower()))
                .OrderByDescending(a => a.DateAdded)
                .Take(3)
                .ToListAsync();

            return Ok(list);
        }


    }
}
