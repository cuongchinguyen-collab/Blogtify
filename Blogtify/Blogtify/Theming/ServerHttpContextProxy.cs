using Blogtify.Client.Theming;

public class ServerHttpContextProxy(IHttpContextAccessor httpContextAccessor)
    : IHttpContextProxy
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Task<string> GetValueAsync(string key)
    {
        return Task.FromResult(_httpContextAccessor.HttpContext?.Request.Cookies[key] ?? string.Empty);
    }

    public Task SetValueAsync(string key, string value, DateTimeOffset? expires = null)
    {
        var options = new CookieOptions
        {
            Expires = expires ?? DateTimeOffset.UtcNow.AddMonths(1),
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(key, value, options);
        return Task.CompletedTask;
    }

    public bool IsSupported() => true;
}
