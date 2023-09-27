using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace RainBot.Channel.Core.Services;
public class TelegramBotService : IBotService
{
    private readonly TelegramBotClient _botClient;
    private readonly string _telegramChannelId;

    public TelegramBotService(string telegramBotToken, string telegramChannelId)
    {
        _botClient = new TelegramBotClient(telegramBotToken);
        _telegramChannelId = telegramChannelId;
    }
    public async Task<string> SendMessageAsync(string messageText, bool isSilent = false)
    {
        var message = await _botClient.SendTextMessageAsync(_telegramChannelId, messageText, parseMode: ParseMode.Markdown, disableWebPagePreview: true, disableNotification: isSilent);

        return message.MessageId.ToString();
    }
    public async Task EditMessageAsync(string messageId, string messageText)
    {
        try
        {
            await _botClient.EditMessageTextAsync(_telegramChannelId, int.Parse(messageId), messageText, ParseMode.Markdown, disableWebPagePreview: true);
        }
        catch (ApiRequestException ex) when (ex.Message.Equals("Bad Request: message is not modified: specified new message content and reply markup are exactly the same as a current content and reply markup of the message", StringComparison.Ordinal))
        {
            // We skip this because it means that the current forecast is already synchronized with Telegram, but our database is not.
            // This happens when we send a Telegram message, the database upsert fails and the function is started for the second time.
        }

    }
    public async Task DeleteMessageAsync(string messageId)
    {
        await _botClient.DeleteMessageAsync(_telegramChannelId, int.Parse(messageId));
    }
}
