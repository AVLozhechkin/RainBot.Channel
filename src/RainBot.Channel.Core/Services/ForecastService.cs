using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Channel.Core.Models;
using RainBot.Channel.Core.Repositories;

namespace RainBot.Channel.Core.Services;

public class ForecastService : IForecastService
{
    private readonly IForecastRepository _forecastRepository;

    public ForecastService(IForecastRepository forecastRepository)
    {
        _forecastRepository = forecastRepository;
    }

    public async Task<IReadOnlyList<Forecast>> GetForecastPairsFromDb(IReadOnlyList<Forecast> forecastsFromApi) =>
        await _forecastRepository.GetForecastsByPKAsync(forecastsFromApi);

    public async Task UpsertForecasts(IReadOnlyList<Forecast> forecasts) =>
        await _forecastRepository.UpsertForecastsAsync(forecasts);
}
