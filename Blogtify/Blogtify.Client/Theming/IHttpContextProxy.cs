namespace Blogtify.Client.Theming;

public interface IHttpContextProxy
{
    bool IsSupported();
    string GetCookieValue(string key);
}