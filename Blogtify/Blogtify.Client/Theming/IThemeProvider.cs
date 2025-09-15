using Havit.Blazor.Components.Web.Bootstrap;

namespace Blogtify.Client.Theming;

public interface IThemeProvider
{
    event ThemeChangedHandler ThemeChanged;

    Theme GetTheme();
    void SetTheme(Theme theme);
}