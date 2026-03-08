using DevBites.Models;

namespace DevBites.Services;

public interface IAIContentService
{
    Task<IReadOnlyList<ProgrammingReel>> GetReelsAsync(string topic, CancellationToken cancellationToken = default);
}
