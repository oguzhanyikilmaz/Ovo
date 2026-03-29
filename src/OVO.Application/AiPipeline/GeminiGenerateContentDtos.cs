using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OVO.AiPipeline;

/// <summary>Gemini generateContent REST gövdesi (camelCase, resmi JS SDK ile uyumlu).</summary>
internal sealed class GeminiGenerateContentRequest
{
    public List<GeminiContentRequest> Contents { get; init; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiGenerationConfigRequest? GenerationConfig { get; init; }
}

internal sealed class GeminiContentRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; init; }

    public List<GeminiPartRequest> Parts { get; init; } = [];
}

internal sealed class GeminiPartRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiInlineDataRequest? InlineData { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Thought { get; init; }
}

internal sealed class GeminiInlineDataRequest
{
    public string MimeType { get; init; } = default!;

    public string Data { get; init; } = default!;
}

internal sealed class GeminiGenerationConfigRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ResponseModalities { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiImageConfigRequest? ImageConfig { get; init; }
}

internal sealed class GeminiImageConfigRequest
{
    public string AspectRatio { get; init; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageSize { get; init; }
}
