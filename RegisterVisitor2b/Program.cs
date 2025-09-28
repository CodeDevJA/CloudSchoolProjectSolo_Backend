using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Is used to create a host builder for Azure Functions that run in an isolated process.
var builder = FunctionsApplication.CreateBuilder(args);

// Sets up the function to handle web requests (HTTP triggers)
builder.ConfigureFunctionsWebApplication();

// Add Application Insights
builder.Services
    // Adds monitoring - like installing security cameras in your restaurant to see what's happening
    .AddApplicationInsightsTelemetryWorkerService()
    // Configures how the monitoring works specifically for Azure Functions
    .ConfigureFunctionsApplicationInsights();

// Build and run the configured Functions application host
var app = builder.Build();

// Start the Functions application and listen for triggers
app.Run();

/* 
Note about CORS in Azure Functions:

For Azure Functions with isolated worker model, CORS is configured in host.json file instead of Program.cs.
The CORS configuration should be moved to host.json like this:

{
    "version": "2.0",
    "logging": {
        "applicationInsights": {
            "samplingSettings": {
                "isEnabled": true,
                "excludedTypes": "Request"
            },
            "enableLiveMetricsFilters": true
        }
    },
    "extensions": {
        "http": {
            "routePrefix": "api"
        }
    },
    "cors": {
        "supportCredentials": true,
        "allowedOrigins": [
            "http://localhost:3000",
            "http://localhost:5173",
            "https://codedevja.github.io"
        ]
    }
}
*/
