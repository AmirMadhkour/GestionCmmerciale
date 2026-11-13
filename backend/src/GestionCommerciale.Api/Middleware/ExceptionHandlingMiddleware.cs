using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GestionCommerciale.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
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
            _logger.LogError(ex, "Une erreur s'est produite : {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        
        var statusCode = exception switch
        {
            BusinessRuleException or ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedException or UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = statusCode == 500 ? "Une erreur interne au serveur s'est produite." : exception.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        var result = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(result);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Erreur de règle métier",
        401 => "Non authentifié",
        403 => "Accès refusé",
        404 => "Ressource introuvable",
        409 => "Conflit de données",
        _ => "Erreur interne du serveur"
    };
}