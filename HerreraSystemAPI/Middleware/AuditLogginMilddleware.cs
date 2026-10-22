using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using HerreraSystem.API.Models;
using HerreraSystem.API.Services;

namespace HerreraSystem.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public AuditLoggingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        var timer = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        // 1. Permitir la lectura múltiple del flujo del Request Body
        context.Request.EnableBuffering();

        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // REBOBINAR el stream para los controladores
        }

        // 2. Sanitizar datos sensibles (Contraseñas, Tokens, CVV)
        requestBody = SanitizarPayload(requestBody);

        Exception? caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\":\"Error interno en el servidor.\", \"traceId\":\"{traceId}\"}}");
        }
        finally
        {
            timer.Stop();

            var log = new SystemLog
            {
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Environment = _env.EnvironmentName,
                Level = caughtException != null ? "Error" : (context.Response.StatusCode >= 400 ? "Warning" : "Information"),
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonimo",
                Request = new RequestDetail
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    Payload = string.IsNullOrWhiteSpace(requestBody) ? null : requestBody
                },
                Response = new ResponseDetail
                {
                    StatusCode = context.Response.StatusCode,
                    ElapsedMilliseconds = timer.ElapsedMilliseconds
                }
            };

            if (caughtException != null)
            {
                log.Exception = new ExceptionDetail
                {
                    Message = caughtException.Message,
                    StackTrace = caughtException.StackTrace,
                    Source = caughtException.Source ?? string.Empty
                };
            }


            _ = logService.InsertLogAsync(log);
        }
    }

    private static string SanitizarPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return payload;

        // Ocultar valores de password, token, clave o secret mediante Regex
        var pattern = @"(?i)""(password|contraseña|token|secret|clave)""\s*:\s*""([^""]+)""";
        return Regex.Replace(payload, pattern, "\"$1\": \"******\"");
    }
}