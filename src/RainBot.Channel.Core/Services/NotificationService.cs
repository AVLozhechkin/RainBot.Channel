using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Services;

public class NotificationService : INotificationService
{
    public const byte PrecipitationThreshold = 20;
    private readonly IBotService _botService;
    private readonly IMessageService _messageService;

    public NotificationService(IBotService botService, IMessageService messageService)
    {
        _botService = botService;
        _messageService = messageService;
    }

    public async Task<IReadOnlyList<Forecast>> SendNotifications(IReadOnlyList<Forecast> previousForecasts, IReadOnlyList<Forecast> currentForecasts)
    {
        var notifications = GetNotifications(previousForecasts, currentForecasts);

        Console.Write($"Processing following notifications: {JsonSerializer.Serialize(notifications)}");

        // Please do not look below

        if (notifications.Count == 2 && notifications[0].MessageId == notifications[1].MessageId)
        {
            // If two notifications without id, then just send double message
            if (notifications[0].MessageId is null)
            {
                string messageText = _messageService.BuildMessage(notifications);
                var messageId = await _botService.SendMessageAsync(messageText);

                notifications.ForEach(n => n.CurrentForecast.ChannelPostId = messageId);

                return currentForecasts;
            }

            var firstNotification = notifications[0].Change;
            var secondNotification = notifications[1].Change;

            if (firstNotification == secondNotification && firstNotification == NotificationChange.RainConditionChanged)
            {
                string messageText = _messageService.BuildMessage(notifications);
                await _botService.EditMessageAsync(notifications[0].MessageId, messageText);
                return currentForecasts;
            }

            if (firstNotification == secondNotification)
            {
                await _botService.DeleteMessageAsync(notifications[0].MessageId);

                string messageText = _messageService.BuildMessage(notifications);

                var messageId = await _botService.SendMessageAsync(messageText, firstNotification == NotificationChange.RainCanceled);

                notifications.ForEach(n => n.CurrentForecast.ChannelPostId = messageId);

                return currentForecasts;
            }

            if (
                (firstNotification == NotificationChange.RainOccured && secondNotification == NotificationChange.RainConditionChanged) ||
                (firstNotification == NotificationChange.RainConditionChanged && secondNotification == NotificationChange.RainOccured) ||
                (firstNotification == NotificationChange.RainCanceled && secondNotification == NotificationChange.RainConditionChanged) ||
                (firstNotification == NotificationChange.RainConditionChanged && secondNotification == NotificationChange.RainCanceled))
            {
                var editMessage = firstNotification == NotificationChange.RainConditionChanged ? notifications[0] : notifications[1];

                await _botService.EditMessageAsync(editMessage.MessageId, _messageService.BuildMessage(editMessage));

                var sendMessage = firstNotification == NotificationChange.RainConditionChanged ? notifications[1] : notifications[0];

                sendMessage.CurrentForecast.ChannelPostId = await _botService
                    .SendMessageAsync(
                        _messageService.BuildMessage(sendMessage),
                        sendMessage.Change == NotificationChange.RainCanceled);

                return currentForecasts;
            }

            if ((firstNotification == NotificationChange.RainOccured && secondNotification == NotificationChange.RainCanceled) ||
                (firstNotification == NotificationChange.RainCanceled && secondNotification == NotificationChange.RainOccured))
            {
                var editMessage = firstNotification == NotificationChange.RainCanceled ? notifications[0] : notifications[1];
                string messageText = _messageService.BuildMessage(editMessage);
                Console.WriteLine(JsonSerializer.Serialize(editMessage));
                await _botService.EditMessageAsync(editMessage.MessageId, messageText);

                var sendMessage = firstNotification == NotificationChange.RainCanceled ? notifications[1] : notifications[0];
                messageText = _messageService.BuildMessage(sendMessage);
                sendMessage.CurrentForecast.ChannelPostId = await _botService.SendMessageAsync(messageText);

                return currentForecasts;
            }

            return currentForecasts;
        }
        else
        {
            foreach (var notification in notifications)
            {
                var messageText = _messageService.BuildMessage(notification);

                var isCurrentRainy = notification.CurrentForecast.PrecipitationProbability >= PrecipitationThreshold;
                var isPreviousRainy = notification.PreviousForecast?.PrecipitationProbability >= PrecipitationThreshold;

                // DB null, Api rain = send new message                                   
                // DB clear without messageId, Api rain = send new message                     
                if ((notification.Change == NotificationChange.RainOccured && string.IsNullOrWhiteSpace(notification.MessageId)) //||
                   // (notification.PreviousForecast is null && isCurrentRainy) ||
                    //(!isPreviousRainy && isCurrentRainy)
                    )
                {
                    notification.CurrentForecast.ChannelPostId = await _botService.SendMessageAsync(messageText);

                    continue;
                }

                // DB rain, Api rain (different conditions)
                if (notification.Change == NotificationChange.RainConditionChanged)
                {
                    await _botService.EditMessageAsync(notification.MessageId, messageText);

                    continue;
                }

                // DB rain, Api clear = delete old, send new silent message               - DELETE, SEND SILENT
                // DB clear (with message id), Api rain = delete old, send new message    - DELETE, SEND
                if (isPreviousRainy ^ isCurrentRainy)
                {
                    await _botService.DeleteMessageAsync(notification.MessageId);

                    notification.CurrentForecast.ChannelPostId = await _botService.SendMessageAsync(messageText, !isCurrentRainy);
                }
            }

            return currentForecasts;
        }
    }

    private static List<Notification> GetNotifications(IReadOnlyList<Forecast> previousForecasts, IReadOnlyList<Forecast> currentForecasts)
    {
        Guard.IsNotNull(currentForecasts);
        Guard.IsNotNull(previousForecasts);

        var notifications = new List<Notification>(2);

        foreach (var apiForecast in currentForecasts)
        {
            var dbForecast = previousForecasts.FirstOrDefault(wr => wr.Date == apiForecast.Date && wr.DayTime == apiForecast.DayTime);

            apiForecast.ChannelPostId = dbForecast?.ChannelPostId;

            var rainInApi = apiForecast.PrecipitationProbability >= PrecipitationThreshold;
            var rainInDb = dbForecast?.PrecipitationProbability >= PrecipitationThreshold;

            // DB null, Api clear
            // DB clear, Api clear
            // DB rain, Api rain (same conditions)
            // No notifications required
            if ((dbForecast is null && !rainInApi) ||
                (!rainInDb && !rainInApi) ||
                (rainInApi && rainInDb && apiForecast.Condition == dbForecast?.Condition))
            {
                continue;
            }

            notifications.Add(new Notification
            {
                MessageId = apiForecast.ChannelPostId,
                CurrentForecast = apiForecast,
                PreviousForecast = dbForecast,
                Change = GetNotificationChange(dbForecast, apiForecast)
            });

        }

        return notifications;
    }

    private static NotificationChange GetNotificationChange(Forecast previousForecast, Forecast currentForecast)
    {
        if (previousForecast is null)
        {
            return NotificationChange.New;
        }

        if (previousForecast.PrecipitationProbability >= PrecipitationThreshold &&
            currentForecast.PrecipitationProbability >= PrecipitationThreshold &&
            previousForecast.Condition != currentForecast.Condition)
        {
            return NotificationChange.RainConditionChanged;
        }

        if (previousForecast.PrecipitationProbability >= PrecipitationThreshold &&
            currentForecast.PrecipitationProbability < PrecipitationThreshold)
        {
            return NotificationChange.RainCanceled;
        }

        if (currentForecast.PrecipitationProbability >= PrecipitationThreshold)
        {
            return NotificationChange.RainOccured;
        }

        throw new ArgumentException($"Unknown change: prev - {previousForecast.PrecipitationProbability} {previousForecast.Condition}, curr - {currentForecast.PrecipitationProbability} {currentForecast.Condition}");
    }
}
