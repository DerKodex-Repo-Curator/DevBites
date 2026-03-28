using DevBites.Models;

namespace DevBites.Services;

public interface IFavoritesService
{
    string GetReelId(ProgrammingReel reel);
    Task<IReadOnlyCollection<string>> GetFavoriteIdsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgrammingReel>> GetFavoritesAsync(CancellationToken cancellationToken = default);
    Task<bool> ToggleFavoriteAsync(ProgrammingReel reel, CancellationToken cancellationToken = default);
}
