using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelTraining.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected async Task<IActionResult> ExecuteAsync(Func<Task<object>> action)
        {
            try
            {
                var result = await action();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.StackTrace });
            }
        }
    }
}
