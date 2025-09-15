namespace Blogtify.Client.Theming;

public class WebAssemblyHttpContextProxy : IHttpContextProxy
{
    public bool IsSupported() => false;
    public string GetCookieValue(string key) => throw new NotSupportedException();
}