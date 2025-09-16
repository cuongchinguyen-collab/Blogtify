namespace Blogtify.Client.Theming;

public interface IHttpContextProxy
{
    Task<string> GetValueAsync(string key);
    Task SetValueAsync(string key, string value, DateTimeOffset? expires = null);
    bool IsSupported();
}