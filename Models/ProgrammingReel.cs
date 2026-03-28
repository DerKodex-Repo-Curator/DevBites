using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevBites.Models;

public partial class ProgrammingReel : ObservableObject
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

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isFavorite;
}
