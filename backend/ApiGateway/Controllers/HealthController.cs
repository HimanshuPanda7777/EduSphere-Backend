using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HealthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Aggregated health check for all services.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAggregatedHealth()
    {
        var services = new Dictionary<string, string>
        {
            { "identity", "http://localhost:5001/health" },
            { "course", "http://localhost:5002/health" },
            { "core", "http://localhost:5003/health" }
        };

        var results = new Dictionary<string, object>();
        var allHealthy = true;

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        foreach (var service in services)
        {
            try
            {
                var response = await client.GetAsync(service.Value);
                if (response.IsSuccessStatusCode)
                {
                    results[service.Key] = new { status = "healthy" };
                }
                else
                {
                    results[service.Key] = new { status = "unhealthy", statusCode = (int)response.StatusCode };
                    allHealthy = false;
                }
            }
            catch (Exception ex)
            {
                results[service.Key] = new { status = "unreachable", error = ex.Message };
                allHealthy = false;
            }
        }

        var healthResponse = new
        {
            status = allHealthy ? "healthy" : "degraded",
            service = "ApiGateway",
            timestamp = DateTime.UtcNow,
            services = results
        };

        return allHealthy ? Ok(healthResponse) : StatusCode(503, healthResponse);
    }
}
