using DevBites.Models;

namespace DevBites.Services;

public interface IAIContentService
{
    Task<IReadOnlyList<ProgrammingReel>> GetReelsAsync(string topic, string expertiseLevel, CancellationToken cancellationToken = default);
}
