using System.Text.Json.Serialization;

namespace DevBites.Models;

public class ProgrammingReel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("hook")]
    public string Hook { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("codeExample")]
    public string CodeExample { get; set; } = string.Empty;

    [JsonPropertyName("keyTakeaway")]
    public string KeyTakeaway { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}
