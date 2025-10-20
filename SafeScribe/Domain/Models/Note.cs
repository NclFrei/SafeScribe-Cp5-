namespace SafeScribe.Domain.Models;

public class Note
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid UserId { get; set; }
}
