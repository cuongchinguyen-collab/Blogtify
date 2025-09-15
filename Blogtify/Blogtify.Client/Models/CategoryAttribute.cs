namespace Blogtify.Client.Models;

public class CategoryAttribute : Attribute
{
    public string Category { get; set; } = string.Empty;
    public string? ParentCategory { get; set; }
    public CategoryAttribute(string category, string? parentCategory = null)
    {
        Category = category;
        ParentCategory = parentCategory;
    }
}
