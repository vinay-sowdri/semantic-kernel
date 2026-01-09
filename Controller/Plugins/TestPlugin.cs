using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelTraining.TestPlugin
{
    public class TestPlugin
    {
        [KernelFunction("get_city_summary")]
        [Description("Get a summary of the city provided")]
        public async Task<string> GetCitySummary(Kernel kernel, [Description("The name of the city to summarize")] string city)
        {
            Console.WriteLine($"[Plugin] Received city parameter: '{city}'");
            const string summarizePrompt = @"
            You are a travel assistant. Mention the things to do for the mentioned city in one sentence:
            {{$city}}
            ";

            var arguments = new KernelArguments() {{"city", city }};
            Console.WriteLine($"[Plugin] Prompt template: {summarizePrompt}");
            Console.WriteLine($"[Plugin] Arguments: city='{arguments["city"]}'");

            var result = kernel.CreateFunctionFromPrompt(
                            summarizePrompt,
                            functionName: "SummarizeCity"
                        );
            Console.WriteLine($"[Plugin] Invoking AI with the prompt...", result);
            var invocationResult = await result.InvokeAsync(kernel, arguments);
                        
            Console.WriteLine($"[Plugin] Result received from AI", invocationResult);
            return invocationResult.ToString();
        }
    }
}