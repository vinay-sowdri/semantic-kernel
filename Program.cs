using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
using SemanticKernelTraining.Plugins;
using SemanticKernelTraining.SummarizePlugin;
using SemanticKernelTraining.CustomerPlugin;
using SemanticKernelTraining.ChatPromptHistoryPlugin;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticKernelTraining.TestPlugin;
using SemanticKernelTraining.Configuration;
using Microsoft.Extensions.Options;
using SemanticKernelTraining.Interface;
using SemanticKernelTraining.FlightToolPlugin;
using SemanticKernelTraining.FlightFilterPlugin;
using SemanticKernelTraining.Filters;

// Use built-in logging instead of Console.WriteLine
var builder = WebApplication.CreateBuilder(args);

// Configure standard logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Register configuration
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection(OpenAIOptions.OpenAI));
builder.Services.AddSingleton<IFlightService, FlightService>();
// Validate configuration
var openAIOptions = builder.Configuration.GetSection(OpenAIOptions.OpenAI).Get<OpenAIOptions>();
if (openAIOptions == null || string.IsNullOrEmpty(openAIOptions.ApiKey) || string.IsNullOrEmpty(openAIOptions.Endpoint))
{
    throw new InvalidOperationException("OpenAI configuration is missing or invalid.");
}

var endpoint = new Uri(openAIOptions.Endpoint);


// Register core services
builder.Services.AddSingleton<IPromptTemplateFactory, HandlebarsPromptTemplateFactory>();

// Configure OpenAI client
var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
var client = new OpenAIClient(new ApiKeyCredential(openAIOptions.ApiKey), clientOptions);

// Register Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(openAIOptions.ModelId, client);

// Add global function invocation filter
kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, FunctionLoggingFilter>();

// Add plugins to the kernel
kernelBuilder.Plugins.AddFromType<DishListPlugin>();
kernelBuilder.Plugins.AddFromType<SummarizePlugin>();
kernelBuilder.Plugins.AddFromType<CustomerPlugin>();
kernelBuilder.Plugins.AddFromType<ChatPromptHistoryPlugin>();
kernelBuilder.Plugins.AddFromType<TestPlugin>();
kernelBuilder.Plugins.AddFromType<FlightToolPlugin>();
kernelBuilder.Plugins.AddFromType<FlightFilterPlugin>();

builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add standard exception handling middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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