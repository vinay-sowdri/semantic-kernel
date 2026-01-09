namespace SemanticKernelTraining.Constants
{
    public static class Constants
    {
        public static string Template = @"
Hello {{customer.first_name}} {{customer.last_name}}!

You are a valued customer (status: {{customer.membership}}).  
Here's your personalized summary:  
{{#each orders}}
  • Order {{this.id}} — {{this.product}} at €{{this.total}}
{{/each}}

Thanks!
";
    }
}