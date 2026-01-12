using Microsoft.SemanticKernel;
using System.Diagnostics;

namespace SemanticKernelTraining.Filters
{
    /// <summary>
    /// Filter that logs function invocations and checks permissions
    /// </summary>
    public class FunctionLoggingFilter : IFunctionInvocationFilter
    {
        private readonly ILogger<FunctionLoggingFilter> _logger;
        private readonly Dictionary<string, HashSet<string>> _allowedFunctions;

        /// <summary>
        /// Initializes a new instance of FunctionLoggingFilter
        /// </summary>
        /// <param name="logger">Logger instance for structured logging</param>
        public FunctionLoggingFilter(ILogger<FunctionLoggingFilter> logger)
        {
            _logger = logger;

            // Initialize hardcoded permissions
            _allowedFunctions = new Dictionary<string, HashSet<string>>
            {
                {
                    "FlightFilterPlugin",
                    new HashSet<string>
                    {
                        "get_flight_details"  // Allowed
                        // "book_flight" is NOT in the list, so it's denied
                    }
                }
            };
        }

        /// <summary>
        /// Intercepts function invocations to check permissions and log execution
        /// </summary>
        /// <param name="context">Function invocation context</param>
        /// <param name="next">Next handler in the pipeline</param>
        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next)
        {
            var pluginName = context.Function.PluginName;
            var functionName = context.Function.Name;

            // Check permissions FIRST (before logging)
            if (!HasUserPermission(pluginName, functionName))
            {
                _logger.LogWarning(
                    "Permission denied for function: {PluginName}.{FunctionName}",
                    pluginName,
                    functionName);

                context.Result = new FunctionResult(context.Result,
                    "The operation was not approved by the user");
                return;
            }

            // Log before function invocation
            _logger.LogInformation(
                "Invoking function: {PluginName}.{FunctionName} with arguments: {Arguments}",
                pluginName,
                functionName,
                SerializeArguments(context.Arguments));

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Call the actual function
                await next(context);

                stopwatch.Stop();

                // Log after successful invocation
                _logger.LogInformation(
                    "Function {PluginName}.{FunctionName} completed in {Duration}ms. Result: {Result}",
                    pluginName,
                    functionName,
                    stopwatch.ElapsedMilliseconds,
                    context.Result?.ToString() ?? "null");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "Function {PluginName}.{FunctionName} failed after {Duration}ms",
                    pluginName,
                    functionName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }

        /// <summary>
        /// Checks if user has permission to execute the specified function
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="functionName">Function name</param>
        /// <returns>True if allowed, false if denied</returns>
        private bool HasUserPermission(string pluginName, string functionName)
        {
            // Only check permissions for FlightFilterPlugin
            if (pluginName != "FlightFilterPlugin")
            {
                return true; // Allow all other plugins
            }

            // Check if the function is in the allowed list
            if (_allowedFunctions.TryGetValue(pluginName, out var allowedFuncs))
            {
                return allowedFuncs.Contains(functionName);
            }

            // If plugin not in dictionary, deny by default for FlightFilterPlugin
            return false;
        }

        /// <summary>
        /// Serializes kernel arguments to readable format
        /// </summary>
        /// <param name="arguments">Kernel arguments</param>
        /// <returns>Serialized string representation</returns>
        private string SerializeArguments(KernelArguments arguments)
        {
            // Serialize arguments to readable format
            return string.Join(", ", arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }
}
