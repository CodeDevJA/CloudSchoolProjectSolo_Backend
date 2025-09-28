using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Is used to create a host builder for Azure Functions that run in an isolated process.
var builder = FunctionsApplication.CreateBuilder(args);

// Sets up the function to handle web requests (HTTP triggers)
builder.ConfigureFunctionsWebApplication();

// Configure CORS - Adds the CORS service to your application
builder.Services.AddCors(options =>
{
    // Creates a named rule called "AllowFrontend"
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Lists which websites are allowed to connect
        policy.WithOrigins(
                // React dev server - Common for React development
                "http://localhost:3000",

                // Vite dev server - Common for Vite/Vue development
                "http://localhost:5173",

                // Production frontend URL - Your actual website URL
                // "https://yourfrontend.com", 

                // My Frontend URL - hosted on GitHub Pages
                "https://github.com/CodeDevJA/CloudSchoolProjectSolo_Frontend.git"
            )
            // Permits all HTTP methods (GET, POST, PUT, DELETE, etc)
            .AllowAnyMethod()

            // Accepts any headers the frontend sends
            .AllowAnyHeader()

            // Allows cookies and authentication (cookies/auth) tokens
            .AllowCredentials();
    });
});

// Add Application Insights
builder.Services

    // Adds monitoring - like installing security cameras in your restaurant to see what's happening
    .AddApplicationInsightsTelemetryWorkerService()

    // Configures how the monitoring works specifically for Azure Functions
    .ConfigureFunctionsApplicationInsights();

// Build the configured Functions application host
var app = builder.Build();

// Use CORS middleware - Actually applies the CORS policy
app.UseCors("AllowFrontend");

// Start the Functions application and listen for triggers
app.Run();


/* 
Note! 

What it does:
- Creates a "builder" (like a construction foreman who knows how to build restaurants)
- Configures the web application
- Adds services one by one
- Builds and runs in one final step

The process: 
Create Builder → Configure → Add Services → Build & Start

Order Matters: 
app.UseCors() must come after builder.Build() but before app.Run()

This setup will allow the frontend to successfully make requests to the Azure Function backend! 
*/
