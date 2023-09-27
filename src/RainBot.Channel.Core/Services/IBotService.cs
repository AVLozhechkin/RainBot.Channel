using System.Threading.Tasks;

namespace RainBot.Channel.Core.Services;

public interface IBotService
{
    Task<string> SendMessageAsync(string messageText, bool isSilent = false);
    Task EditMessageAsync(string messageId, string messageText);
    Task DeleteMessageAsync(string messageId);
}
