namespace DevBites;

public static class Constants
{
    // --- Provider selection (gemini | ollama) ---
    public static string AiProvider =>
        Environment.GetEnvironmentVariable("AI_PROVIDER") ?? "gemini";

    // --- Gemini ---
    public static string GeminiApiKey =>
        Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? throw new InvalidOperationException(
            "GEMINI_API_KEY is not set. Add it to your .env file.");

    public static string GeminiModel =>
        Environment.GetEnvironmentVariable("GEMINI_MODEL")
        ?? "gemini-2.0-flash";

    // --- Ollama ---
    public static string OllamaEndpoint =>
        Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")
        ?? "http://localhost:11434";

    public static string OllamaModel =>
        Environment.GetEnvironmentVariable("OLLAMA_MODEL")
        ?? "llama3.2";
}
