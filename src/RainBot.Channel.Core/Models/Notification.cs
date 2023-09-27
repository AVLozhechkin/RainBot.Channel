namespace RainBot.Channel.Core.Models;

public record Notification
{
    public string MessageId { get; set; }
    public Forecast PreviousForecast { get; set; }
    public Forecast CurrentForecast { get; set; }
    public NotificationChange Change { get; set; }
}

public enum NotificationChange
{
    RainConditionChanged,
    RainCanceled,
    RainOccured,
    New
}
