using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blogtify.Client.Theming;

public class ThemeProvider : IDisposable, IThemeProvider
{
    private readonly IHttpContextProxy _httpContextProxy;
    private readonly PersistentComponentState _persistentComponentState;
    private readonly IJSRuntime _jsRuntime;
    private PersistingComponentStateSubscription _persistingComponentStateSubscription;
    private Theme? _theme;

    public ThemeProvider(
        IHttpContextProxy httpContextProxy,
        PersistentComponentState persistentComponentState,
        IJSRuntime jsRuntime)
    {
        _httpContextProxy = httpContextProxy;
        _persistentComponentState = persistentComponentState;
        _jsRuntime = jsRuntime;
        _persistingComponentStateSubscription = _persistentComponentState.RegisterOnPersisting(PersistTheme);
    }

    public event ThemeChangedHandler? ThemeChanged;

    public async Task SetThemeAsync(Theme theme)
    {
        _theme = theme;
        ThemeChanged?.Invoke(theme);

        await _httpContextProxy.SetValueAsync("Theme", theme.ToString());

        await _jsRuntime.InvokeVoidAsync("themeSwitcher.setTheme", theme.ToString());
    }

    public async Task<Theme> GetThemeAsync()
    {
        if (_theme is null)
        {
            await ResolveInitialTheme();
        }

        return _theme!.Value;
    }

    private async Task ResolveInitialTheme()
    {
        if (_httpContextProxy.IsSupported()
            && await _httpContextProxy.GetValueAsync("Theme") is string cookie
            && Enum.TryParse<Theme>(cookie, ignoreCase: true, out var theme))
        {
            _theme = theme;
        }
        else if (_persistentComponentState.TryTakeFromJson<Theme>("Theme", out var restored))
        {
            _theme = restored;
        }
        else
        {
            _theme = Theme.Yeti;
        }
    }

    private async Task PersistTheme()
    {
        _persistentComponentState.PersistAsJson("Theme", await GetThemeAsync());
    }

    public void Dispose()
    {
        _persistingComponentStateSubscription.Dispose();
    }
}

public delegate void ThemeChangedHandler(Theme theme);
