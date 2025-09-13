namespace Blogtify.Client.Models;

public class TagsAttribute : Attribute
{
    public string Tags { get; set; } = string.Empty;
    public TagsAttribute(string tags)
    {
        Tags = tags;
    }
}
