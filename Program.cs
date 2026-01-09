using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
using SemanticKernelTraining.DishListPlugin;
using SemanticKernelTraining.SummarizePlugin;
using SemanticKernelTraining.CustomerPlugin;
using SemanticKernelTraining.ChatPromptHistoryPlugin;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticKernelTraining.TestPlugin;

Console.WriteLine("Starting application...");

var builder = WebApplication.CreateBuilder(args);

// Read configuration values
var configuration = builder.Configuration;
var githubPat = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured");
var modelId = configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("OpenAI:ModelId not configured");
var endpointStr = configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("OpenAI:Endpoint not configured");
var endpoint = new Uri(endpointStr);

Console.WriteLine("Configuring services...");

// Register core services
builder.Services.AddSingleton<IPromptTemplateFactory, HandlebarsPromptTemplateFactory>();


// Configure OpenAI client
Console.WriteLine("Creating OpenAI client...");
var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
var client = new OpenAIClient(new ApiKeyCredential(githubPat), clientOptions);

// Register Semantic Kernel - it shares the same DI container
Console.WriteLine("Configuring Semantic Kernel...");
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(modelId, client);

// Add plugins to the kernel
kernelBuilder.Plugins.AddFromType<DishListPlugin>();
kernelBuilder.Plugins.AddFromType<SummarizePlugin>();
kernelBuilder.Plugins.AddFromType<CustomerPlugin>();
kernelBuilder.Plugins.AddFromType<ChatPromptHistoryPlugin>();
kernelBuilder.Plugins.AddFromType<TestPlugin>();

builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"EXCEPTION IN PIPELINE: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message, type = ex.GetType().Name });
    }
});

app.UseHttpsRedirection();

app.MapGet("/health", () => "OK").WithName("Health");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (Kernel kernel) =>
{
   try
   {
       var result = await kernel.InvokePromptAsync("Give me a list of 10 breakfast foods with eggs and cheese");
       return Results.Ok(new { success = true, data = result.ToString() });
   }
   catch (Exception ex)
   {
       return Results.BadRequest(new { error = ex.Message, details = ex.StackTrace });
   }
})
.WithName("GetWeatherForecast");

app.MapControllers();

try
{
    Console.WriteLine("About to call app.Run()...");
    app.Run();
    Console.WriteLine("app.Run() completed normally");
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex}");
    throw;
}
finally
{
    Console.WriteLine("Finally block executed");
}