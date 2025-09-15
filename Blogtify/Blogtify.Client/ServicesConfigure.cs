using Blogtify.Client.Theming;
using Havit.Blazor.Components.Web;

namespace Blogtify.Client;

public static class ServicesConfigure
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddHxServices();
        services.AddScoped<IThemeProvider, ThemeProvider>();
    }
}
