using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using SemanticKernelTraining.Constants;
using SemanticKernelTraining.Models;
namespace SemanticKernelTraining.ChatPromptHistoryPlugin
{
    /// <summary>
    /// Plugin for processing chat prompt history with customer context
    /// </summary>
    public class ChatPromptHistoryPlugin
    {
        private readonly IPromptTemplateFactory _templateFactory;

        /// <summary>
        /// Initializes a new instance of ChatPromptHistoryPlugin
        /// </summary>
        /// <param name="templateFactory">Prompt template factory for rendering templates</param>
        public ChatPromptHistoryPlugin(IPromptTemplateFactory templateFactory)
        {
            _templateFactory = templateFactory;
        }

        /// <summary>
        /// Gets chat prompt history for a customer using Handlebars templates
        /// </summary>
        /// <param name="kernel">Semantic Kernel instance</param>
        /// <param name="customer">Customer information including chat history</param>
        /// <returns>AI-generated response based on customer context and history</returns>
        [KernelFunction("get_chat_prompt_history")]
        [Description("Get chat prompt history of the customer")]
        public async Task<string> GetChatPromptHistory(Kernel kernel, [Description("The customer chat prompt history" )] Customer customer)
        {
            Console.WriteLine($"[Plugin] Received customer parameter: '{customer}'");

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "SampleHandleBarTemplate.yaml");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found at path: {templatePath}");
            }
            string yaml = File.ReadAllText(templatePath);
           
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
           var function =  kernel.CreateFunctionFromPrompt(promptConfig, _templateFactory);
           var result = await kernel.InvokeAsync(function, arguments);
            Console.WriteLine($"[Plugin] Prompt template: {yaml}");
            Console.WriteLine($"[Plugin] Arguments: customer='{arguments["customer"]}'");
            
          

                        
            Console.WriteLine($"[Plugin] Result received from AI");
            return result.ToString();
        }
    }
}