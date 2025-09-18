using Blogtify.Client.Models;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Blogtify.Client.Services;

public static class AppDataManager
{
    public static List<CategoryDto> AllCategories => GetAllCategories();

    public static int GetTotalPosts(string query, List<string> categories)
    {
        var posts = GetAllPosts();

        query ??= string.Empty;

        if (categories == null || categories.Count == 0)
        {
            return posts.Count(p => (p.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var allowed = BuildAllowedCategorySet(categories);

        return posts.Count(p =>
            (p.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            && p.Category != null
            && allowed.Contains(p.Category.Name));
    }

    public static List<PostDto> GetPosts(int page, int pageSize, string query, List<string> categories)
    {
        var posts = GetAllPosts().AsEnumerable();

        query ??= string.Empty;

        if (categories != null && categories.Count > 0)
        {
            var allowed = BuildAllowedCategorySet(categories);
            posts = posts.Where(p => p.Category != null && allowed.Contains(p.Category.Name));
        }

        posts = posts.Where(p => (p.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

        return posts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private static HashSet<string> BuildAllowedCategorySet(List<string> selectedCategoryNames)
    {
        var res = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sel in selectedCategoryNames)
        {
            var node = AppDataManager.AllCategories
                        .FirstOrDefault(c => string.Equals(c.Name, sel, StringComparison.OrdinalIgnoreCase));

            if (node != null)
            {
                foreach (var name in GetDescendantNames(node))
                {
                    res.Add(name);
                }
            }
            else
            {
                res.Add(sel);
            }
        }

        return res;
    }

    private static IEnumerable<string> GetDescendantNames(CategoryDto category)
    {
        yield return category.Name;

        var children = category.GetSubCategories();
        if (children == null) yield break;

        foreach (var child in children)
        {
            foreach (var n in GetDescendantNames(child))
                yield return n;
        }
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