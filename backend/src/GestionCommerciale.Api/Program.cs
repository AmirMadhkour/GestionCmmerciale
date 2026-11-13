using GestionCommerciale.Api.Extensions;
using GestionCommerciale.Api.Filters;
using GestionCommerciale.Api.Middleware;
using GestionCommerciale.Api.Services;
using GestionCommerciale.Application;
using GestionCommerciale.Application.Common.Interfaces;
using GestionCommerciale.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

const string corsPolicy = "AllowAngular";

// 1. Controllers (le ValidationFilter gère la validation FluentValidation)
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();

// 2. Injection des couches
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Extensions API
builder.Services.AddCustomCors(corsPolicy);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAndSeedDatabaseAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program {}