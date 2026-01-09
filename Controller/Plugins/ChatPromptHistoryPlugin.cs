using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using SemanticKernelTraining.Constants;
using SemanticKernelTraining.Models;
namespace SemanticKernelTraining.ChatPromptHistoryPlugin
{
    public class ChatPromptHistoryPlugin
    {
        [KernelFunction("get_chat_prompt_history")]
        [Description("Get chat prompt history of the customer")]
        public async Task<string> GetChatPromptHistory(Kernel kernel, [Description("The customer chat prompt history" )] Customer customer)
        {
            Console.WriteLine($"[Plugin] Received customer parameter: '{customer}'");
            
            string yaml = File.ReadAllText("SampleHandleBarTemplate.yaml");
           
             var promptConfig = new PromptTemplateConfig()
        {
            TemplateFormat = "handlebars",  // CRITICAL: Tells SK this is Handlebars
            Template = yaml ,
            Name = "chatPromptHistory",
            InputVariables =
            [
                new() { Name = "customer", Description = "Customer details", AllowDangerouslySetContent = true },
                new() { Name = "history",   Description = "List of history",   AllowDangerouslySetContent = true }
            ]     
            };
           
            var arguments = new KernelArguments();
            arguments["customer"] = new {
                first_name = customer.FirstName,
                last_name = customer.LastName,
                membership = customer.Membership
            };
            arguments["history"] = new [] {
                new {
                    Role = "User",
                    Content = "What are my order details?"
                },
                new {
                    Role = "System",
                    Content = "Could you please provide your membership information?"
                }
            };
           var function =  kernel.CreateFunctionFromPrompt(promptConfig);
           var result = await kernel.InvokeAsync(function, arguments);
            Console.WriteLine($"[Plugin] Prompt template: {yaml}");
            Console.WriteLine($"[Plugin] Arguments: customer='{arguments["customer"]}'");
            
          

                        
            Console.WriteLine($"[Plugin] Result received from AI");
            return result.ToString();
        }
    }
}