using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using SemanticKernelTraining.Constants;
using SemanticKernelTraining.Models;
using System.Reflection.Metadata;
using Microsoft.SemanticKernel.ChatCompletion;
namespace SemanticKernelTraining.CustomerPlugin
{
    public class CustomerPlugin
    {
        private readonly IPromptTemplateFactory _templateFactory;

        public CustomerPlugin(IPromptTemplateFactory templateFactory)
        {
            _templateFactory = templateFactory;
        }

        [KernelFunction("get_customer_details")]
        [Description("Get customer details")]
        public async Task<string> GetCustomerDetails(Kernel kernel, [Description("The customer details" )] Customer customer)
        {
            Console.WriteLine($"[Plugin] Received customer parameter: '{customer}'");
        Console.WriteLine("✓ Template factory created (Handlebars engine ready)");

        // ═══════════════════════════════════════════════════════════
        // STEP 3: Create Prompt Template Configuration
        // ═══════════════════════════════════════════════════════════
        // This config object tells Semantic Kernel:
        // 1. What format the template is in (Handlebars, not Liquid or plain text)
        // 2. What the actual template string is
        //
        // This is just configuration - still not executing anything
        var promptConfig = new PromptTemplateConfig()
        {
            TemplateFormat = "handlebars",  // CRITICAL: Tells SK this is Handlebars
            Template = SemanticKernelTraining.Constants.Constants.Template ,
            Name = "GenerateOrderSummary",
            InputVariables =
            [
                new() { Name = "customer", Description = "Customer details", AllowDangerouslySetContent = true },
                new() { Name = "orders",   Description = "List of orders",   AllowDangerouslySetContent = true }
            ]             // The template string from Step 1
        };
        
        Console.WriteLine("✓ Prompt config created (template + format specified)");

        // ═══════════════════════════════════════════════════════════
        // STEP 4: Create the Prompt Template Object
        // ═══════════════════════════════════════════════════════════
        // Now we combine the factory (the engine) with the config (the template)
        // This creates a PromptTemplate object that CAN render the template
        // But it still hasn't rendered anything yet!
        //
        // This is like loading a program into memory but not running it yet
        var promptTemplate = _templateFactory.Create(promptConfig);
        
        Console.WriteLine("✓ Prompt template object created (ready to render)");

        // ═══════════════════════════════════════════════════════════
        // STEP 5: Prepare the Arguments (Data)
        // ═══════════════════════════════════════════════════════════
        // KernelArguments is a dictionary-like object that holds your data
        // It maps variable names (keys) to actual values
        //
        // This is the DATA that will fill in the template placeholders
        var arguments = new KernelArguments();
        
        // Add the "customer" object
        // This is an anonymous object with properties that match
        // what the template expects: first_name, last_name, membership
        //
        // When template says {{customer.first_name}}, it will get "Alice"
        arguments["customer"] = new {
            first_name = "Alice",      // Will replace {{customer.first_name}}
            last_name = "O'Connor",    // Will replace {{customer.last_name}}
            membership = "Gold"        // Will replace {{customer.membership}}
        };
        
        // Add the "orders" array
        // This is an array of anonymous objects
        // The {{#each orders}} loop will iterate over this array
        //
        // For each item in the array:
        // - {{this.id}} will get the id value (1001, then 1002)
        // - {{this.product}} will get the product name
        // - {{this.total}} will get the price
        arguments["orders"] = new[]{
            new { id = 1001, product = "Laptop", total = 999.99 },
            new { id = 1002, product = "Mouse", total = 49.99 }
        };
        
        Console.WriteLine("✓ Arguments prepared:");
        Console.WriteLine($"  - Customer: Alice O'Connor (Gold member)");
        Console.WriteLine($"  - Orders: 2 items");
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════
        // STEP 6: RenderAsync - THE MAGIC HAPPENS HERE
        // ═══════════════════════════════════════════════════════════
        // THIS is where the template gets executed!
        //
        // What RenderAsync does step by step:
        // 1. Takes the template: "Hello {{customer.first_name}}..."
        // 2. Takes the arguments: customer = { first_name: "Alice", ... }
        // 3. Processes each placeholder:
        //    - Finds {{customer.first_name}}
        //    - Looks up arguments["customer"].first_name
        //    - Replaces {{customer.first_name}} with "Alice"
        // 4. Processes the {{#each orders}} loop:
        //    - Gets the orders array from arguments
        //    - For FIRST order (Laptop):
        //      • Order {{this.id}} becomes • Order 1001
        //      • {{this.product}} becomes Laptop
        //      • €{{this.total}} becomes €999.99
        //    - For SECOND order (Mouse):
        //      • Order {{this.id}} becomes • Order 1002
        //      • {{this.product}} becomes Mouse
        //      • €{{this.total}} becomes €49.99
        // 5. Returns the complete rendered string
        //
        // IMPORTANT: This does NOT call the LLM!
        // It ONLY renders the template into a final string
        string renderedPrompt = await promptTemplate.RenderAsync(kernel, arguments);
        
        // Let's see what the rendered prompt looks like
        Console.WriteLine("=== RENDERED PROMPT (Final String) ===");
        Console.WriteLine(renderedPrompt);
        Console.WriteLine("=======================================\n");
        
        // The rendered prompt will look like:
        // Hello Alice O'Connor!
        // You are a valued customer (status: Gold).
        // Here's your personalized summary:
        //   • Order 1001 — Laptop at €999.99
        //   • Order 1002 — Mouse at €49.99
        // Thanks!

        // ═══════════════════════════════════════════════════════════
        // STEP 7: Send to the LLM
        // ═══════════════════════════════════════════════════════════
        // NOTE: The original code had "kernel.RunAsync(renderedPrompt)"
        // but that method doesn't exist in current Semantic Kernel
        //
        // The correct way to send the rendered prompt to the LLM is:
        
        // Option A: Use Chat Completion Service (recommended)
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(renderedPrompt);  // Add our rendered prompt
        
        var response = await chatService.GetChatMessageContentAsync(chatHistory);
        
        Console.WriteLine("=== LLM RESPONSE ===");
        Console.WriteLine(response.Content);
        Console.WriteLine("====================\n");
        return response.Content;
        }
    }
}