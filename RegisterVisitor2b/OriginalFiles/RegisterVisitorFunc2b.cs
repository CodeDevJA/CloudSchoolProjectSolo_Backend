// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;

// namespace RegisterVisitor2b;

// public class RegisterVisitorFunc2b
// {
//     private readonly ILogger<RegisterVisitorFunc2b> _logger;

//     public RegisterVisitorFunc2b(ILogger<RegisterVisitorFunc2b> logger)
//     {
//         _logger = logger;
//     }

//     [Function("RegisterVisitorFunc2b")]
//     public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
//     {
//         _logger.LogInformation("C# HTTP trigger function processed a request.");
//         return new OkObjectResult("Welcome to Azure Functions!");
//     }
// }
