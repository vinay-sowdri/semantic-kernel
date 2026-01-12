using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelTraining.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for API controllers
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BaseApiController
        /// </summary>
        /// <param name="logger">Logger instance for logging errors and information</param>
        protected BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes an asynchronous action with standardized error handling
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>IActionResult with success response or error details</returns>
        protected async Task<IActionResult> ExecuteAsync(Func<Task<object>> action)
        {
            try
            {
                var result = await action();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the request.");
                // SECURITY: Do NOT return StackTrace to client
                return BadRequest(new { error = "An internal error occurred. Please check logs for details." });
            }
        }
    }
}
