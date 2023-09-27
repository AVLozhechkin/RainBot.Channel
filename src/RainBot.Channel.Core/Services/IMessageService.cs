using System.Collections.Generic;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Services;
public interface IMessageService
{
    public string BuildMessage(Notification notification);
    public string BuildMessage(IReadOnlyList<Notification> notifications);
}
