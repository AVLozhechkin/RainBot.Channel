using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainBot.Channel.Core.Models;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace RainBot.Channel.Core.Repositories;

public class ForecastRepository : IForecastRepository
{
    private readonly Driver _driver;

    public ForecastRepository(Driver driver)
    {
        _driver = driver;
    }
    public async Task<IReadOnlyList<Forecast>> GetForecastsByPKAsync(IReadOnlyList<Forecast> forecasts)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $date1 AS Date;
DECLARE $dayTime1 as Uint8;
DECLARE $date2 AS Date;
DECLARE $dayTime2 as Uint8;

SELECT * FROM forecasts
WHERE date = $date1 and dayTime = $dayTime1 or date = $date2 and dayTime = $dayTime2
";

        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$date1", YdbValue.MakeDate(forecasts[0].Date.DateTime) },
                        { "$dayTime1", YdbValue.MakeUint8((byte) forecasts[0].DayTime) },
                        { "$date2", YdbValue.MakeDate(forecasts[1].Date.DateTime) },
                        { "$dayTime2", YdbValue.MakeUint8((byte) forecasts[1].DayTime) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        response.Status.EnsureSuccess();

        var queryResponse = (ExecuteDataQueryResponse)response;

        if (queryResponse.Result.ResultSets.Count == 0)
        {
            return Array.Empty<Forecast>();
        }

        var forecastsFromDatabase = new List<Forecast>();

        foreach (var row in queryResponse.Result.ResultSets[0].Rows)
        {
            var weatherRecord = new Forecast
            {
                Date = row["date"].GetDate(),
                DayTime = (DayTime)row["dayTime"].GetUint8(),
                Condition = row["condition"].GetOptionalUtf8(),
                ChannelPostId = row["channelPostId"].GetOptionalUtf8(),
                PrecipitationProbability = row["precipitationProbability"].GetOptionalUint8().Value,
                UpdatedAt = row["updatedAt"].GetOptionalDatetime().Value,
            };

            forecastsFromDatabase.Add(weatherRecord);
        }

        return forecastsFromDatabase;
    }
    public async Task UpsertForecastsAsync(IReadOnlyList<Forecast> forecasts)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $forecasts AS List<Struct<
    date: Date,
    dayTime: Uint8,
    condition: Utf8,
    channelPostId: Utf8,
    precipitationProbability: Uint8,
    updatedAt: Datetime>>;

UPSERT INTO forecasts
SELECT * FROM AS_TABLE($forecasts);
";
        var forecastsData = forecasts.Select(forecast => YdbValue.MakeStruct(new Dictionary<string, YdbValue>
        {
            { "date", YdbValue.MakeDate(forecast.Date.Date) },
            { "dayTime", YdbValue.MakeUint8((byte) forecast.DayTime) },
            { "condition", YdbValue.MakeUtf8(forecast.Condition) },
            { "channelPostId", YdbValue.MakeUtf8(forecast.ChannelPostId ?? string.Empty) },
            { "precipitationProbability", YdbValue.MakeUint8(forecast.PrecipitationProbability) },
            { "updatedAt", YdbValue.MakeDatetime(forecast.UpdatedAt.DateTime) },
        })).ToList();

        var forecastsList = new Dictionary<string, YdbValue>()
        {
            { "$forecasts", YdbValue.MakeList(forecastsData) },
        };

        var sessionResult = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
            query: query,
            parameters: forecastsList,
            txControl: TxControl.BeginSerializableRW().Commit()
        );
        });

        sessionResult.Status.EnsureSuccess();
    }
}
