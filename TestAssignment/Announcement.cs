namespace TestAssignment
{
    public class Announcement
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}
