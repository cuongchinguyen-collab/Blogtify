using Blogtify.Client.Services;

namespace Blogtify.Client.Models;

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Parent { get; set; }

    public List<CategoryDto> GetSubCategories()
    {
        return AppDataManager.AllCategories
            .Where(c => c.Parent != null && c.Parent.Equals(Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public override bool Equals(object? obj)
    {
        if (obj is CategoryDto other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
