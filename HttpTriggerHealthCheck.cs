using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TigerStride.ContactSvc
{
    /// <summary>
    /// CheckHealth endpoint for the Contact Service.
    /// </summary> 
    public partial class HttpTriggerContact
    {
        [Function("CheckHealth")]
        public IActionResult CheckHealth([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Health check requested.");
                return new OkObjectResult(new { status = "Healthy" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
