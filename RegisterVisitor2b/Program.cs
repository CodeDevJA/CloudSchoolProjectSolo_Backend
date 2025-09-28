using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                // React dev server
                "http://localhost:3000",

                // Vite dev server
                "http://localhost:5173",

                // Production frontend URL
                // "https://yourfrontend.com", 

                // My Frontend URL - hosted on GitHub Pages
                "https://github.com/CodeDevJA/CloudSchoolProjectSolo_Frontend.git"
            )
            .AllowAnyMethod()              // GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader()              // Accept any headers
            .AllowCredentials();           // Allow cookies/auth tokens
    });
});

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();

// Use CORS middleware
app.UseCors("AllowFrontend");

app.Run();