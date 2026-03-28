using System.Security.Cryptography;
using System.Text;
using DevBites.Models;
using SQLite;

namespace DevBites.Services;

public class FavoritesService : IFavoritesService
{
    private readonly SQLiteAsyncConnection _database;
    private bool _isInitialized;

    public FavoritesService()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "devbites.db3");
        _database = new SQLiteAsyncConnection(databasePath);
    }

    public string GetReelId(ProgrammingReel reel)
    {
        var seed = $"{reel.Title}|{reel.Hook}|{reel.Explanation}|{reel.CodeExample}|{reel.KeyTakeaway}|{reel.Language}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexString(hash);
    }

    public async Task<IReadOnlyCollection<string>> GetFavoriteIdsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync().WaitAsync(cancellationToken);
        var items = await _database.Table<FavoriteReelEntity>().ToListAsync().WaitAsync(cancellationToken);
        return items.Select(x => x.Id).ToArray();
    }

    public async Task<IReadOnlyList<ProgrammingReel>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync().WaitAsync(cancellationToken);

        var entities = await _database
            .Table<FavoriteReelEntity>()
            .OrderByDescending(x => x.SavedAtUtc)
            .ToListAsync()
            .WaitAsync(cancellationToken);

        return entities.Select(MapToReel).ToArray();
    }

    public async Task<bool> ToggleFavoriteAsync(ProgrammingReel reel, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync().WaitAsync(cancellationToken);

        var id = GetReelId(reel);
        var existing = await _database.FindAsync<FavoriteReelEntity>(id).WaitAsync(cancellationToken);

        if (existing is not null)
        {
            await _database.DeleteAsync(existing).WaitAsync(cancellationToken);
            return false;
        }

        await _database.InsertAsync(MapToEntity(id, reel)).WaitAsync(cancellationToken);
        return true;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized)
            return;

        await _database.CreateTableAsync<FavoriteReelEntity>();
        _isInitialized = true;
    }

    private static FavoriteReelEntity MapToEntity(string id, ProgrammingReel reel) => new()
    {
        Id = id,
        Title = reel.Title,
        Hook = reel.Hook,
        Explanation = reel.Explanation,
        CodeExample = reel.CodeExample,
        KeyTakeaway = reel.KeyTakeaway,
        Language = reel.Language,
        SavedAtUtc = DateTime.UtcNow
    };

    private static ProgrammingReel MapToReel(FavoriteReelEntity entity) => new()
    {
        Title = entity.Title,
        Hook = entity.Hook,
        Explanation = entity.Explanation,
        CodeExample = entity.CodeExample,
        KeyTakeaway = entity.KeyTakeaway,
        Language = entity.Language,
        IsFavorite = true
    };
}
