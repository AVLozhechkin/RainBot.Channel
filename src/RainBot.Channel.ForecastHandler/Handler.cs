using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Channel.Core.Models;
using RainBot.Channel.Core.Models.Functions;
using RainBot.Channel.Core.Repositories;
using RainBot.Channel.Core.Services;
using RainBot.Channel.Core.Utils;
using Yandex.Cloud.Functions;

namespace RainBot.Channel.ForecastHandler;
public class Handler
{
    private readonly string _latitude = Environment.GetEnvironmentVariable("LATITUDE");
    private readonly string _longitude = Environment.GetEnvironmentVariable("LONGITUDE");
    private readonly string _databasePath = Environment.GetEnvironmentVariable("YDB_DATABASE");
    private readonly string _telegramToken = Environment.GetEnvironmentVariable("TG_TOKEN");
    private readonly string _telegramChannel = Environment.GetEnvironmentVariable("TG_CHANNEL_ID");

    public Handler()
    {
        Guard.IsNotNullOrWhiteSpace(_databasePath);
        Guard.IsNotNullOrWhiteSpace(_latitude);
        Guard.IsNotNullOrWhiteSpace(_longitude);
        Guard.IsNotNullOrWhiteSpace(_telegramToken);
        Guard.IsNotNullOrWhiteSpace(_telegramChannel);

    }

    public async Task<Response> FunctionHandler(QueueRequest request, Context context)
    {
        Guard.IsNotNull(request);
        Guard.IsNotNull(context);

        var serviceToken = JsonSerializer.Deserialize<ServiceToken>(context.TokenJson);
        Guard.IsNotNullOrWhiteSpace(serviceToken.AccessToken);

        var forecastsFromApi = JsonSerializer.Deserialize<IReadOnlyList<Forecast>>(request.Messages[0].Details.Message.Body);
        Guard.IsNotNull(forecastsFromApi);
        Guard.HasSizeEqualTo(forecastsFromApi, 2);

        using var driver = DriverExtensions.Build(_databasePath, serviceToken.AccessToken);
        await driver.Initialize().ConfigureAwait(false);

        var forecastRepository = new ForecastRepository(driver);
        var telegramBotService = new TelegramBotService(_telegramToken, _telegramChannel);
        var messageService = new MessageService(_latitude, _longitude);

        var notificationService = new NotificationService(telegramBotService, messageService);
        var forecastService = new ForecastService(forecastRepository);

        var forecastsFromDb = await forecastService.GetForecastPairsFromDb(forecastsFromApi).ConfigureAwait(false);
        Console.WriteLine("Forecasts from database: " + JsonSerializer.Serialize(forecastsFromDb));

        var updatedForecastsFromApi = await notificationService.SendNotifications(forecastsFromDb, forecastsFromApi).ConfigureAwait(false);
        Console.WriteLine("Updated forecasts: " + JsonSerializer.Serialize(updatedForecastsFromApi));

        await forecastService.UpsertForecasts(updatedForecastsFromApi).ConfigureAwait(false);


        return new Response(200, string.Empty);
    }
}
