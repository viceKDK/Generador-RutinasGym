namespace GymRoutineGenerator.Infrastructure.AI.Models;

public class OllamaRequest
{
    public string Model { get; set; } = "mistral:7b";
    public string Prompt { get; set; } = string.Empty;
    public bool Stream { get; set; } = false;
    public OllamaOptions Options { get; set; } = new();
}

public class OllamaOptions
{
    public double Temperature { get; set; } = 0.7;
    public int Top_p { get; set; } = 1;
    public int Max_tokens { get; set; } = 2000;
    public string[] Stop { get; set; } = Array.Empty<string>();
}