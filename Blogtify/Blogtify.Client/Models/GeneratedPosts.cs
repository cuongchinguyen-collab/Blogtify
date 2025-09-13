using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Blogtify.Client.Models;

public static class GeneratedPosts
{
    public static List<PostDto> All => GetPosts();

    private static List<PostDto> GetPosts()
    {
        return Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(CustomComponentBase).IsAssignableFrom(t)
                            && t.GetCustomAttribute<RouteAttribute>() is not null)
                        .Select(t =>
                        {
                            var route = t.GetCustomAttribute<RouteAttribute>()?.Template ?? t.Name;
                            var title = t.Name.Replace("_", " ");

                            return new PostDto()
                            {
                                Title = title,
                                Route = route
                            };
                        })
                        .ToList();
    }
}