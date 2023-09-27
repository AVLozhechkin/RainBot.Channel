using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Services;

public interface INotificationService
{
    Task<IReadOnlyList<Forecast>> SendNotifications(IReadOnlyList<Forecast> previousForecasts, IReadOnlyList<Forecast> currentForecasts);
}
