using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
using SemanticKernelTraining.DishListPlugin;
using SemanticKernelTraining.SummarizePlugin;
using SemanticKernelTraining.CustomerPlugin;
using SemanticKernelTraining.ChatPromptHistoryPlugin;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
Console.WriteLine("Starting application...");

// Create the ASP.NET Core app builder first to access configuration
var builder = WebApplication.CreateBuilder(args);

// Read configuration values
var configuration = builder.Configuration;
var githubPat = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured");
var modelId = configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("OpenAI:ModelId not configured");
var endpointStr = configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("OpenAI:Endpoint not configured");
var endpoint = new Uri(endpointStr);

Console.WriteLine("Creating OpenAI client...");
// Use the standard OpenAI client, but with GitHub credentials
var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
var client = new OpenAIClient(new ApiKeyCredential(githubPat), clientOptions);

Console.WriteLine("Creating kernel...");
// Create the Kernel
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(modelId, client);

// Add plugins at startup (not per-request)
Console.WriteLine("Adding plugins...");
kernelBuilder.Plugins.AddFromType<DishListPlugin>();
kernelBuilder.Plugins.AddFromType<SummarizePlugin>();
kernelBuilder.Plugins.AddFromType<CustomerPlugin>();
kernelBuilder.Plugins.AddFromType<ChatPromptHistoryPlugin>();

var kernel = kernelBuilder.Build();

// Register services BEFORE building the app
builder.Services.AddSingleton<Kernel>(kernel);
builder.Services.AddSingleton<IPromptTemplateFactory, HandlebarsPromptTemplateFactory>();
builder.Services.AddSingleton<DishListPlugin>();
builder.Services.AddSingleton<SummarizePlugin>();
builder.Services.AddSingleton<CustomerPlugin>();
builder.Services.AddSingleton<ChatPromptHistoryPlugin>();
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

app.MapGet("/weatherforecast", async () =>
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