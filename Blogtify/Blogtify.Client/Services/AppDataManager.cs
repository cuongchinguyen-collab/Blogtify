using Blogtify.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;

namespace Blogtify.Client.Services;

public static class AppDataManager
{
    public static List<CategoryDto> AllCategories => GetAllCategories();

    public static int GetTotalPosts(string query, List<string> categories)
    {
        return GetAllPosts()
            .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Where(p => categories.Count == 0 || categories.Any(c => p.Category != null && p.Category.Name.ToLower() == c.ToLower()))
            .Count();
    }

    public static List<PostDto> GetPosts(int page, int pageSize, string query, List<string> categories)
    {
        return GetAllPosts()
                .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Where(p => categories.Count == 0 || categories.Any(c => p.Category != null && p.Category.Name.ToLower() == c.ToLower()))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
    }

    public static PostDto? GetPostByRoute(string route)
    {
        return GetAllPosts().FirstOrDefault(p => p.Route.Equals(route, StringComparison.OrdinalIgnoreCase));
    }

    private static List<PostDto> GetAllPosts()
    {
        return Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(CustomComponentBase).IsAssignableFrom(t)
                            && t.GetCustomAttribute<RouteAttribute>() is not null)
                        .Select(t =>
                        {
                            var route = t.GetCustomAttribute<RouteAttribute>()?.Template ?? t.Name;
                            var title = t.Name.Replace("_", " ");
                            var category = t.GetCustomAttribute<CategoryAttribute>()?.Category;
                            var parentCategory = t.GetCustomAttribute<CategoryAttribute>()?.ParentCategory;

                            return new PostDto()
                            {
                                Title = title,
                                Route = route,
                                Category = !string.IsNullOrEmpty(category) ? new CategoryDto
                                {
                                    Name = category.ToLower(),
                                    Parent = parentCategory?.ToLower()
                                } : null
                            };
                        })
                        .ToList();
    }

    private static List<CategoryDto> GetAllCategories()
    {
        var categories = GetAllPosts()
                    .Select(p => p.Category)
                    .Where(c => c != null)
                    .ToHashSet()!;

        var categoriesNames = categories
                    .Select(c => c!.Name.ToLower())
                    .ToHashSet()!;

        var parentCategories = categories
                    .Select(c => c!.Parent)
                    .Where(pc => pc != null)
                    .Select(pc => pc!.ToLower())
                    .ToHashSet()!;

        if (parentCategories.Except(categoriesNames).Any())
        {
            throw new Exception($"These parent categories do not exist: {string.Join(",", parentCategories)}");
        }

        return categories.ToList()!;
    }
}