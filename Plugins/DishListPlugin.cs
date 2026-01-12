using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelTraining.Plugins
{
    public class DishListPlugin
    {
        [KernelFunction("get_dish_list")]
        [Description("Get a Dish List based on the ingredients provided")]
        public async Task<string> GetDishList(Kernel kernel, [Description("The main ingredient (e.g., egg, chicken, potato)")] string ingredients)
        {
            Console.WriteLine($"[Plugin] Received ingredients parameter: '{ingredients}'");
            
            var prompt = @"You are a world class Indian chef. Given the main ingredient, provide a list of 10 dishes that can be made using this main ingredient.
                         Ingredients: {{$ingredients}}";

            var arguments = new KernelArguments() {{"ingredients", ingredients }};
            Console.WriteLine($"[Plugin] Prompt template: {prompt}");
            Console.WriteLine($"[Plugin] Arguments: ingredients='{arguments["ingredients"]}'");
            
            var result = await kernel.InvokePromptAsync(prompt, arguments);
            Console.WriteLine($"[Plugin] Result received from AI");
            return result.ToString();
        }
    }
}