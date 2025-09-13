using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Blogtify.Client.Models;

public static class AppDataManager
{
    public static List<PostDto> AllPosts => GetAllPosts();
    public static List<string> AllTags => GetAllTags();

    public static int GetTotalPosts(string query, List<string> tags)
    {
        return AllPosts
            .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Where(p => tags.Count == 0 || tags.Any(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
            .Count();
    }

    public static List<PostDto> GetPosts(int page, int pageSize, string query, List<string> tags)
    {
        return AllPosts
                .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Where(p => tags.Count == 0 || tags.Any(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
    }

    public static PostDto? GetPostByRoute(string route)
    {
        return AllPosts.FirstOrDefault(p => p.Route.Equals(route, StringComparison.OrdinalIgnoreCase));
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
                            var tags = t.GetCustomAttribute<TagsAttribute>()?.Tags.Split(",") ?? [];

                            return new PostDto()
                            {
                                Title = title,
                                Route = route,
                                Tags = tags.ToList()
                            };
                        })
                        .ToList();
    }

    private static List<string> GetAllTags()
    {
        return AllPosts.SelectMany(p => p.Tags).Distinct().ToList();
    }
}