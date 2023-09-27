using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Mapster;
using RainBot.Channel.Core.Services;
using RainBot.Channel.Core.Models;
using MyForecast = RainBot.Channel.Core.Models.Forecast;

namespace RainBot.Channel.YandexWeatherFetcher;

internal sealed class WeatherService
{
    private readonly IMessageQueueService _ymqService;
    private readonly HttpClient _http;
    private readonly Uri _forecastHandlerQueue;

    internal WeatherService(IMessageQueueService ymqService, HttpClient http, Uri forecastHandlerQueue)
    {
        _ymqService = ymqService;
        _http = http;
        _forecastHandlerQueue = forecastHandlerQueue;
    }
    internal async Task FetchAndForwardForecastAsync(string latitude, string longitude)
    {
        var result = await _http.GetFromJsonAsync<InformersResponse>($"https://api.weather.yandex.ru/v2/informers?lat={latitude}&lon={longitude}");

        var forecasts = MapPartsToForecasts(result.Forecast);

        await _ymqService.SendMessageAsync(forecasts, _forecastHandlerQueue);
    }

    private static MyForecast[] MapPartsToForecasts(Forecast forecast)
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var weatherRecords = new MyForecast[2];

        var currentTime = updatedAt.AddHours(3);

        for (var i = 0; i < forecast.Parts.Count; i++)
        {
            var forecastPart = forecast.Parts[i];

            var weatherRecord = new MyForecast
            {
                Date = currentTime.Date,
                UpdatedAt = updatedAt,
                ChannelPostId = string.Empty
            };

            forecastPart.Adapt(weatherRecord);

            // If it is between 12 and 18 hours, then we need to add 1 day to Night forecast. Or if it is between 18 and 24 then we need to add 1 day to both records

            if ((currentTime.Hour is >= 12 and < 18 && weatherRecord.DayTime == DayTime.Night) ||
                currentTime.Hour is >= 18 and < 24)
            {
                weatherRecord.Date = weatherRecord.Date.AddDays(1);
            }

            weatherRecords[i] = weatherRecord;
        }

        return weatherRecords;
    }
}
