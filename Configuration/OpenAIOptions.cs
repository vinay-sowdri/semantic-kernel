namespace SemanticKernelTraining.Configuration
{
    public class OpenAIOptions
    {
        public const string OpenAI = "OpenAI";

        public string ApiKey { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}
