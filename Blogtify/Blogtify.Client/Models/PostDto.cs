namespace Blogtify.Client.Models;

public class PostDto
{
    public string Route { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public CategoryDto? Category { get; set; }
}
