using SQLite;

namespace DevBites.Models;

public class FavoriteReelEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string CodeExample { get; set; } = string.Empty;
    public string KeyTakeaway { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime SavedAtUtc { get; set; }
}
