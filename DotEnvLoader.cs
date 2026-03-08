using System.Reflection;

namespace DevBites;

internal static class DotEnvLoader
{
    public static void Load()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(".env", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            return;

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            // Skip blank lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            // Only set if not already present (allows host env to override)
            if (!string.IsNullOrEmpty(key) && Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
