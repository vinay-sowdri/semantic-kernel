using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelTraining.SummarizePlugin
{
    public class SummarizePlugin
    {
        [KernelFunction("get_Summarize_details")]
        [Description("Get a summary of the topic provided")]
        public async Task<string> GetSummarizeDetails(Kernel kernel, [Description("The Summary of the topic provided" )] string topic)
        {
            Console.WriteLine($"[Plugin] Received topic parameter: '{topic}'");
            const string summarizePrompt = @"
            You are a concise assistant. Summarize the following text in one sentence:
            {{$topic}}
            ";
           

            var arguments = new KernelArguments() {{"topic", topic }};
            Console.WriteLine($"[Plugin] Prompt template: {summarizePrompt}");
            Console.WriteLine($"[Plugin] Arguments: topic='{arguments["topic"]}'");
            
            var result = await kernel.CreateFunctionFromPrompt(
                            summarizePrompt,
                            functionName: "SummarizeText"
                        ).InvokeAsync(kernel, arguments);

                        
            Console.WriteLine($"[Plugin] Result received from AI");
            return result.ToString();
        }
    }
}