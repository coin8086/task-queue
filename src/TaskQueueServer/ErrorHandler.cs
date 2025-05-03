namespace Rz.TaskQueue.Server;

using System.Net.Mime;

public class ErrorHandler
{
    private IWebHostEnvironment _hostEnv;
    private RequestDelegate _next;
    private ILogger _logger;

    public ErrorHandler(IWebHostEnvironment hostEnv, RequestDelegate next, ILogger<ErrorHandler> logger)
    {
        _hostEnv = hostEnv;
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error!");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = MediaTypeNames.Text.Plain;

            string result;
            if (_hostEnv.IsDevelopment())
            {
                result = ex.ToString();
            }
            else
            {
                result = "Server error!";
            }
            await context.Response.WriteAsync(result);
        }
    }
}
