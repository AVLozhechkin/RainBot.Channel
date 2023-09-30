using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Diagnostics;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Services;
public class MessageService : IMessageService
{
    private readonly string _latitude;
    private readonly string _longitude;
    private static CultureInfo _cultureInfo = new CultureInfo("ru-RU");
    private const string DateFormat = "d MMMM";

    public MessageService(string latitude, string longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
    }
    public string BuildMessage(Notification notification)
    {
        var text = GetYandexPrefix() + BuildMessageForSingleNotification(notification);

        if (notification.PreviousForecast is not null && notification.Change == NotificationChange.RainCanceled)
        {
            return BuildSingleNoRainPrefix(notification) + text;
        }

        return text;
    }
    public string BuildMessage(IReadOnlyList<Notification> notifications)
    {
        var text = GetYandexPrefix();

        text += BuildMessageForTwoNotifications(notifications);

        if (notifications[0].Change == NotificationChange.RainCanceled && notifications[0].Change == notifications[1].Change)
        {
            return BuildDoubleNoRainPrefix(notifications) + text;
        }

        if (notifications[0].Change == NotificationChange.RainCanceled)
        {
            return BuildSingleNoRainPrefix(notifications[0]) + text;
        }

        if (notifications[1].Change == NotificationChange.RainCanceled)
        {
            return BuildSingleNoRainPrefix(notifications[1]) + text;
        }

        return text;
    }

    private static string BuildMessageForTwoNotifications(IReadOnlyList<Notification> notifications)
    {
        Guard.IsNotNull(notifications);
        Guard.IsNotEmpty(notifications);
        Guard.IsNotNull(notifications[0].CurrentForecast);
        Guard.IsNotNull(notifications[1].CurrentForecast);

        if (notifications[0].CurrentForecast.Condition == notifications[1].CurrentForecast.Condition)
        {
            return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForSameConditions],
            MessageStrings.RussianDayTimes.Value[notifications[0].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[0].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianDayTimes.Value[notifications[1].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[1].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[0].CurrentForecast.Condition].ToLowerInvariant());
        }

        return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForDifferentConditions],
            MessageStrings.RussianConditions.Value[notifications[0].CurrentForecast.Condition].ToLowerInvariant(),
            MessageStrings.RussianDayTimes.Value[notifications[0].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[0].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianDayTimes.Value[notifications[1].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[1].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[1].CurrentForecast.Condition].ToLowerInvariant());
    }

    private static string BuildMessageForSingleNotification(Notification notification)
    {
        Guard.IsNotNull(notification);
        Guard.IsNotNull(notification.CurrentForecast);

        return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForOneRecord],
            MessageStrings.RussianDayTimes.Value[notification.CurrentForecast.DayTime].ToLowerInvariant(),
            notification.CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notification.CurrentForecast.Condition].ToLowerInvariant()
        );
    }

    private static string BuildDoubleNoRainPrefix(IReadOnlyList<Notification> notifications)
    {
        Guard.IsNotNull(notifications);
        Guard.IsNotEmpty(notifications);
        Guard.IsNotNull(notifications[0].PreviousForecast);
        Guard.IsNotNull(notifications[1].PreviousForecast);

        if (notifications[0].PreviousForecast.Condition != notifications[1].PreviousForecast.Condition)
        {
            return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.DoubleNoDifferentRainsPrefix],
            MessageStrings.RussianDayTimes.Value[notifications[0].CurrentForecast.DayTime],
            notifications[0].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[0].CurrentForecast.Condition].ToLowerInvariant(),
            MessageStrings.RussianDayTimes.Value[notifications[1].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[1].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[1].CurrentForecast.Condition].ToLowerInvariant());
        }

        if (notifications[0].CurrentForecast.Date.Day == notifications[1].CurrentForecast.Date.Day)
        {
            return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.DoubleNoSameRainsSameDatePrefix],
            MessageStrings.RussianDayTimes.Value[notifications[0].CurrentForecast.DayTime],
            MessageStrings.RussianDayTimes.Value[notifications[1].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[0].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[1].CurrentForecast.Condition].ToLowerInvariant()
        );
        }

        return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.DoubleNoSameRainsPrefix],
            MessageStrings.RussianDayTimes.Value[notifications[0].CurrentForecast.DayTime],
            notifications[0].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianDayTimes.Value[notifications[1].CurrentForecast.DayTime].ToLowerInvariant(),
            notifications[1].CurrentForecast.Date.ToString(DateFormat, _cultureInfo),
            MessageStrings.RussianConditions.Value[notifications[1].CurrentForecast.Condition].ToLowerInvariant()
        );
    }

    private string GetYandexPrefix()
        => MessageStrings.GetPrefix(_latitude, _longitude);

    private static string BuildSingleNoRainPrefix(Notification notification)
    {
        Guard.IsNotNull(notification);
        Guard.IsNotNull(notification.PreviousForecast);

        return string.Format(
            MessageStrings.RussianMessages.Value[MessageTypes.SingleNoRainPrefix],
            MessageStrings.RussianConditions.Value[notification.PreviousForecast.Condition],
            MessageStrings.RussianDayTimes.Value[notification.PreviousForecast.DayTime].ToLowerInvariant(),
            notification.CurrentForecast.Date.ToString(DateFormat, _cultureInfo));
    }
}
