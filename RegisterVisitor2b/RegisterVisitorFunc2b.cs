using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Npgsql;
using System.Net;

namespace RegisterVisitor2b
{
    public class RegisterVisitorFunc2b
    {
        // Nested Service - via Dependency injection for Logging 
        private readonly ILogger<RegisterVisitorFunc2b> _logger;

        // Constructor - Dependency injection for Logging 
        // Enables the Azure Application Insights Monitoring
        public RegisterVisitorFunc2b(ILogger<RegisterVisitorFunc2b> logger)
        {
            _logger = logger;
        }

        // HttpTrigger Function/Endpoint
        [Function("RegisterVisitorFunc2b")]
        // Allows public/anonymous users/visitors to register at the frontend-page (via DTO-request) and connect to the backend
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contact")] HttpRequestData req)
        {
            // Monitoring output
            _logger.LogInformation("C# HTTP trigger function processed a visitor registration request.");

            // Create response object (DTO-response, for the frontend-user)
            var response = req.CreateResponse();

            try
            {
                // Parse request body - Read JSON from frontend
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation($"Received registration data: {requestBody}");

                // Deserialize JSON to VisitorRequest object
                var visitorData = JsonSerializer.Deserialize<VisitorRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Handles case differences between frontend/backend
                });

                // Validation process - Guard-clauses
                // Validate JSON data exists
                if (visitorData == null) // If data is missing
                {
                    // Response-StatusCode (400-series), Bad input
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Invalid JSON data received.");
                    return response;
                }

                // Validate required fields (firstname, surname, email are required)
                if (string.IsNullOrWhiteSpace(visitorData.Firstname) ||
                    string.IsNullOrWhiteSpace(visitorData.Surname) ||
                    string.IsNullOrWhiteSpace(visitorData.Email))
                {
                    // Response-StatusCode (400-series), Bad input
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("First name, surname, and email are required fields.");
                    return response;
                }

                // Validate email format
                if (!IsValidEmail(visitorData.Email))
                {
                    // Response-StatusCode (400-series), Bad input
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Please provide a valid email address.");
                    return response;
                }

                // Get database connection string from Azure environment variables
                var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Response-StatusCode (500-series), Backend
                    _logger.LogError("POSTGRES_CONN_STRING is not configured in Azure settings or local.settings.json.");
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    await response.WriteStringAsync("Database configuration error. Please contact support.");
                    return response;
                }

                // Save visitor data to PostgreSQL database
                // Arguments Class/Datatype: 
                // "string" (ConnStr) 
                // "VisitorRequest" (Frontend-input-object)
                await SaveVisitorToDatabase(connectionString, visitorData);

                // Monitoring output
                // Log successful registration
                _logger.LogInformation($"Successfully registered visitor: {visitorData.Firstname} {visitorData.Surname} ({visitorData.Email}) from {visitorData.Company ?? "No Company"}");

                // Send success response to frontend
                // Response-StatusCode (200-series), OK/Successful
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync("Registration successful! Thank you for visiting.");
            }
            catch (JsonException jsonEx)
            {
                // Error handling, (400-series), Bad input
                _logger.LogError($"JSON parsing error: {jsonEx.Message}");
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Invalid data format received.");
            }
            catch (NpgsqlException dbEx)
            {
                // Error handling, (500-series), Backend
                _logger.LogError($"Database error: {dbEx.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Database error occurred. Please try again later.");
            }
            catch (Exception ex)
            {
                // Monitoring output
                _logger.LogError($"Unexpected error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                // Error handling, (500-series), Backend
                response.StatusCode = HttpStatusCode.InternalServerError;
                // Error handling, Generic error message
                await response.WriteStringAsync("An unexpected error occurred. Please try again.");
            }

            // Returms DTO-responce to frontend-user
            return response;
        }

        // Database operations - Save visitor information to PostgreSQL
        private async Task SaveVisitorToDatabase(string connectionString, VisitorRequest visitor)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Create visitors table (schema in AZ Psql-DB) if it doesn't exist 
            // (C#/Psql-syntax)
            using var createTableCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS visitors (
                    id SERIAL PRIMARY KEY,
                    first_name VARCHAR(100) NOT NULL,
                    surname VARCHAR(100) NOT NULL,
                    company VARCHAR(200),
                    email VARCHAR(255) NOT NULL,
                    visit_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )", connection);

            await createTableCmd.ExecuteNonQueryAsync();

            // Insert new visitor record to the AZ Psql-DB 
            // (C#/Psql-syntax)
            using var insertCmd = new NpgsqlCommand(@"
                INSERT INTO visitors (first_name, surname, company, email) 
                VALUES (@firstName, @surname, @company, @email)", connection);

            // Use parameterized queries to prevent SQL injection
            insertCmd.Parameters.AddWithValue("@firstName", visitor.Firstname);
            insertCmd.Parameters.AddWithValue("@surname", visitor.Surname);
            insertCmd.Parameters.AddWithValue("@company", visitor.Company ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@email", visitor.Email);

            await insertCmd.ExecuteNonQueryAsync();
        }

        // Used in validation process - Guard-clauses
        // Email validation helper method
        private static bool IsValidEmail(string email)
        {
            try
            {
                // Use built-in .NET email validation
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    // Data Transfer Object (DTO) - Matches frontend form input-fields (hosted on GitHub Pages)
    // = string.Empty; is set to be empty as Default, when a value is missing
    public class VisitorRequest
    {
        public string Firstname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
