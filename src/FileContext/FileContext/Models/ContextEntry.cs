namespace FileContext.Models;

public class ContextEntry
{
    public long Id { get; set; }

    public string Title { get; set; } = "";

    public string Path { get; set; } = "";

    public string Description { get; set; } = "";

    public string Status { get; set; } = "";

    public string NextAction { get; set; } = "";

    public string Tags { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}