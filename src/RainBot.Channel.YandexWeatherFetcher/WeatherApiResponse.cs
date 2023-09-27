using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RainBot.Channel.YandexWeatherFetcher;

public record InformersResponse
{
    [JsonPropertyName("forecast")] public Forecast Forecast { get; set; }
}

public record Forecast
{
    [JsonPropertyName("parts")] public IReadOnlyList<Part> Parts { get; set; } = new List<Part>();
}

public record Part
{
    [JsonPropertyName("part_name")] public string PartName { get; set; }
    [JsonPropertyName("prec_prob")] public int PrecProb { get; set; }
    [JsonPropertyName("condition")] public string Condition { get; set; }
}
