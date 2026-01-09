
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using SemanticKernelTraining.DishListPlugin;
using SemanticKernelTraining.SummarizePlugin;
using SemanticKernelTraining.CustomerPlugin;
using SemanticKernelTraining.Models;
[ApiController]
[Route("api/[controller]")]
public class DishListController : ControllerBase
{
    private readonly Kernel _kernel;
    public DishListController(Kernel kernel)
    {
        _kernel = kernel;
    }
    [HttpGet("get-dish-list")]
    public async Task<IActionResult> GetDishList([FromQuery] string ingredients){
        try
        {
            Console.WriteLine($"[Controller] Received ingredients: '{ingredients}'");
            
            if (string.IsNullOrWhiteSpace(ingredients))
            {
                return BadRequest(new { error = "ingredients parameter is required and cannot be empty" });
            }

           var result = await _kernel.InvokeAsync("DishListPlugin", "get_dish_list", new KernelArguments() { { "ingredients", ingredients } });
           return Ok(new { success = true, data = result.ToString() });
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpGet("get-summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string topic){
        try
        {
            Console.WriteLine($"[Controller] Received topic: '{topic}'");
            
            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest(new { error = "topic parameter is required and cannot be empty" });
            }

           var result = await _kernel.InvokeAsync("SummarizePlugin", "get_Summarize_details", new KernelArguments() { { "topic", topic } });
           return Ok(new { success = true, data = result.ToString() });
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpGet("get-customer-summary")]
    public async Task<IActionResult> GetCustomerSummary(){
        try
        {
            Customer customer = new Customer
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
            
            if (customer == null)
            {
                return BadRequest(new { error = "customer parameter is required and cannot be null" });
            }

           var result = await _kernel.InvokeAsync("CustomerPlugin", "get_customer_details", new KernelArguments() { { "customer", customer } });
           return Ok(new { success = true, data = result.ToString() });
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.StackTrace });
        }
    }


    [HttpGet("get-customer-chat-history")]
    public async Task<IActionResult> GetChatPromptHistory(){
        try
        {
            Customer customer = new Customer
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
            
            if (customer == null)
            {
                return BadRequest(new { error = "customer parameter is required and cannot be null" });
            }

           var result = await _kernel.InvokeAsync("ChatPromptHistoryPlugin", "get_chat_prompt_history", new KernelArguments() { { "customer", customer } });
           return Ok(new { success = true, data = result.ToString() });
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.StackTrace });
        }
    }
}