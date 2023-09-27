using System;

namespace RainBot.Channel.Core.Models;

public record Forecast
{
    public DateTimeOffset Date { get; set; }
    public DayTime DayTime { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte PrecipitationProbability { get; set; }
    public string Condition { get; set; }
    public string ChannelPostId { get; set; }
}
