using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PromptHQ.API.Middleware;
using PromptHQ.Application;
using PromptHQ.Infrastructure;
using PromptHQ.AI;
using PromptHQ.Communication;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Layers
builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure();
builder.Services.AddCommunication(builder.Configuration);
builder.Services.AddAILayer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}
else
{
    // For production/staging, you might still want Scalar/OpenApi or just the redirect
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

// app.UseHttpsRedirection(); // Removed for Render compatibility (handled by proxy)
app.UseAuthorization();
app.MapControllers();

app.Run();
