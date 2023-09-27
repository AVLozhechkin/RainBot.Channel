using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core.Services;

public interface IForecastService
{
    public Task<IReadOnlyList<Forecast>> GetForecastPairsFromDb(IReadOnlyList<Forecast> forecastsFromApi);
    public Task UpsertForecasts(IReadOnlyList<Forecast> forecasts);
}
