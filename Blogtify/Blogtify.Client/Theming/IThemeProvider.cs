namespace Blogtify.Client.Theming;

public interface IThemeProvider
{
    event ThemeChangedHandler ThemeChanged;

    Task<Theme> GetThemeAsync();
    Task SetThemeAsync(Theme theme);
}