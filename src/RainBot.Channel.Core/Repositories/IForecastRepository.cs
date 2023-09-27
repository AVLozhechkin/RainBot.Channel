using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Repositories;

public interface IForecastRepository
{
    Task UpsertForecastsAsync(IReadOnlyList<Forecast> forecasts);
    Task<IReadOnlyList<Forecast>> GetForecastsByPKAsync(IReadOnlyList<Forecast> forecasts);
}
