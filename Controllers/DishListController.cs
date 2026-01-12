
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using SemanticKernelTraining.Plugins;
using SemanticKernelTraining.FlightToolPlugin;
using SemanticKernelTraining.FlightFilterPlugin;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelTraining.Interface; // Add this
using SemanticKernelTraining.SummarizePlugin;
using SemanticKernelTraining.CustomerPlugin;
using SemanticKernelTraining.Models;
using SemanticKernelTraining.Controllers;

/// <summary>
/// Controller for dish list and AI-related operations
/// </summary>
[Route("api/[controller]")]
public class DishListController : BaseApiController
{
    private readonly Kernel _kernel;
    private readonly ILogger<DishListController> _logger;
    private readonly IFlightService _flightService;

    /// <summary>
    /// Initializes a new instance of DishListController
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="flightService">Flight service instance</param>
    public DishListController(Kernel kernel, ILogger<DishListController> logger, IFlightService flightService) : base(logger)
    {
        _kernel = kernel;
        _logger = logger;
        _flightService = flightService;
    }

    /// <summary>
    /// Gets a list of dishes based on provided ingredients
    /// </summary>
    /// <param name="ingredients">Comma-separated list of ingredients</param>
    /// <returns>AI-generated list of dishes</returns>
    [HttpGet("get-dish-list")]
    public async Task<IActionResult> GetDishList([FromQuery] string ingredients)
    {
        return await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Received ingredients: '{Ingredients}'", ingredients);

            if (string.IsNullOrWhiteSpace(ingredients))
            {
                throw new ArgumentException("ingredients parameter is required and cannot be empty");
            }

            var result = await _kernel.InvokeAsync("DishListPlugin", "get_dish_list", new KernelArguments() { { "ingredients", ingredients } });
            return result.ToString();
        });
    }

    /// <summary>
    /// Gets a summary of a given topic
    /// </summary>
    /// <param name="topic">The topic to summarize</param>
    /// <returns>AI-generated summary</returns>
    [HttpGet("get-summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string topic)
    {
        return await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Received topic: '{Topic}'", topic);

            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("topic parameter is required and cannot be empty");
            }

            var result = await _kernel.InvokeAsync("SummarizePlugin", "get_Summarize_details", new KernelArguments() { { "topic", topic } });
            return result.ToString();
        });
    }

    [HttpGet("get-customer-summary")]
    public async Task<IActionResult> GetCustomerSummary()
    {
        return await ExecuteAsync(async () =>
        {
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Membership = "Gold",
                Orders = new List<Order>
                {
                    new Order { Id = 1, Product = "Laptop", Total = 1200.50m },
                    new Order { Id = 2, Product = "Smartphone", Total = 800.00m }
                }
            };
            Console.WriteLine($"[Controller] Received customer: '{customer}'");

            var result = await _kernel.InvokeAsync("CustomerPlugin", "get_customer_details", new KernelArguments() { { "customer", customer } });
            return result.ToString();
        });
    }


    [HttpGet("get-customer-chat-history")]
    public async Task<IActionResult> GetChatPromptHistory()
    {
        return await ExecuteAsync(async () =>
        {
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Membership = "Gold",
                History = new List<History>
                {
                    new History { Role = "User", Content = "What are my order details?" },
                    new History { Role = "System", Content = "Could you please provide your membership information?" }
                }
            };
            Console.WriteLine($"[Controller] Received customer: '{customer}'");

            var result = await _kernel.InvokeAsync("ChatPromptHistoryPlugin", "get_chat_prompt_history", new KernelArguments() { { "customer", customer } });
            return result.ToString();
        });
    }

    [HttpGet("get-test")]
    public async Task<IActionResult> GetTest([FromQuery] string city)
    {
        return await ExecuteAsync(async () =>
        {

            var result = await _kernel.InvokeAsync("testplugin", "get_city_summary", new KernelArguments() { { "city", city} });
            return result.ToString();
        });
    }

    /// <summary>
    /// Processes an AI request with flight tool integration
    /// </summary>
    /// <param name="request">User request containing the prompt</param>
    /// <returns>AI-generated response</returns>
    [HttpPost("ask")]
    public async Task<IActionResult> AskAI([FromBody] UserRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { error = "Prompt is required" });
        }

        // 1. Clone the kernel to create a scoped instance
        var scopedKernel = _kernel.Clone();

        // 2. Clear unrelated plugins to save tokens
        scopedKernel.Plugins.Clear();

        // Use injected service to create plugin
        scopedKernel.Plugins.AddFromType<FlightToolPlugin>();
        KernelFunction searchFlights = scopedKernel.Plugins.GetFunction("FlightToolPlugin", "get_flight_details");
        // Define execution settings
        OpenAIPromptExecutionSettings settings = new() 
        { 
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions : [searchFlights]) 
        };

        // OpenAIPromptExecutionSettings settings = new() 
        // { 
        //     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        // };

        // Invoke the scoped kernel
        var promptWithContext = $"{request.Prompt} (Today is {DateTime.Now:yyyy-MM-dd})";
        var result = await scopedKernel.InvokePromptAsync(promptWithContext, new(settings));

        return Ok(new { Answer = result.ToString() });
    }

    /// <summary>
    /// Processes an AI request with flight filter plugin (demonstrates global filter with permissions)
    /// </summary>
    /// <param name="request">User request containing the prompt</param>
    /// <returns>AI-generated response</returns>
    [HttpPost("ask-with-filter")]
    public async Task<IActionResult> AskWithFilter([FromBody] UserRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { error = "Prompt is required" });
        }

        // 1. Clone kernel (global filter already registered in Program.cs)
        var scopedKernel = _kernel.Clone();

        // 2. Clear and add plugins
        // Use the plugin instance from the main kernel since it has DI dependencies
        scopedKernel.Plugins.Clear();
        var flightFilterPlugin = _kernel.Plugins.FirstOrDefault(p => p.Name == "FlightFilterPlugin");
        if (flightFilterPlugin != null)
        {
            scopedKernel.Plugins.Add(flightFilterPlugin);
        }


        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // 4. Invoke (global filter will intercept automatically)
        var promptWithContext = $"{request.Prompt} (Today is {DateTime.Now:yyyy-MM-dd})";
        var result = await scopedKernel.InvokePromptAsync(promptWithContext, new(settings));

        return Ok(new { Answer = result.ToString() });
    }
}