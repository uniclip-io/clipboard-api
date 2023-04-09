using Bugsnag;

namespace ClipboardApi.Middlewares;

internal class HttpExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IClient _bugsnag;

    public HttpExceptionMiddleware(RequestDelegate next, IClient bugsnag)
    {
        _next = next;
        _bugsnag = bugsnag;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception exception)
        {
            _bugsnag.Notify(exception);
            context.Response.StatusCode = 500;
        }
    }
}