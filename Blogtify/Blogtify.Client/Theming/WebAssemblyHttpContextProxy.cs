using Microsoft.JSInterop;

namespace Blogtify.Client.Theming;

public class WebAssemblyHttpContextProxy : IHttpContextProxy
{
    private readonly IJSRuntime _jsRuntime;

    public WebAssemblyHttpContextProxy(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsSupported() => true;

    public async Task<string> GetValueAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    public async Task SetValueAsync(string key, string value, DateTimeOffset? expires = null)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }
}
