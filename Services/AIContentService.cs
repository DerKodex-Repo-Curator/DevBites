using DevBites.Models;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace DevBites.Services;

public class AIContentService : IAIContentService
{
    private readonly IChatClient _chatClient;

    private const string PromptTemplate = """
        Generate exactly 10 programming concept cards for the topic: {0}.

        Respond ONLY with a valid JSON array. No markdown, no code fences, no backticks, no extra text.
        The response must start with [ and end with ].

        Use this exact JSON structure for every item:
        [
          {{
            "title": "Short catchy title (5-8 words)",
            "hook": "One powerful attention-grabbing sentence about why this matters",
            "explanation": "2-3 clear sentences explaining the concept for a developer",
            "codeExample": "A short, focused code snippet (5-10 lines max)",
            "keyTakeaway": "The single most important lesson from this concept",
            "language": "{0}"
          }}
        ]

        Rules:
        - Vary the concepts across beginner, intermediate, and advanced topics
        - Keep code examples concise and directly relevant
        - Make the hook genuinely engaging, not generic
        - Return ONLY the JSON array — nothing else
        """;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    public AIContentService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<IReadOnlyList<ProgrammingReel>> GetReelsAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(PromptTemplate, topic);

        var response = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User, prompt)],
            cancellationToken: cancellationToken);

        var json = response.Text
            ?? throw new InvalidOperationException("Received an empty response from the AI service.");

        json = ExtractJsonArray(json);

        var reels = JsonSerializer.Deserialize<List<ProgrammingReel>>(json, _jsonOptions);
        return reels ?? [];
    }

    private static string ExtractJsonArray(string raw)
    {
        var start = raw.IndexOf('[');
        var end = raw.LastIndexOf(']');

        if (start >= 0 && end > start)
            return raw[start..(end + 1)];

        return raw.Trim();
    }
}
