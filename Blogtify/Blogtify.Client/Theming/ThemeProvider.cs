using Havit.Blazor.Components.Web.Bootstrap;
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

    public event ThemeChangedHandler ThemeChanged;

    public void SetTheme(Theme theme)
    {
        _theme = theme;
        ThemeChanged?.Invoke(theme);

        _ = _jsRuntime.InvokeVoidAsync("themeSwitcher.setTheme", theme.ToString());
    }

    public Theme GetTheme()
    {
        if (_theme == null)
        {
            ResolveInitialTheme();
        }
        return _theme.Value;
    }

    private void ResolveInitialTheme()
    {
        // prerendering
        if (_httpContextProxy.IsSupported()
            && _httpContextProxy.GetCookieValue("Theme") is string cookie
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

    private Task PersistTheme()
    {
        _persistentComponentState.PersistAsJson("Theme", GetTheme());
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        _persistingComponentStateSubscription.Dispose();
    }
}

public delegate void ThemeChangedHandler(Theme theme);