using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Diagnostics;
using RainBot.Channel.Core.Models;

namespace RainBot.Channel.Core;

public static class MessageStrings
{
    public static string GetPrefix(string latitude, string longitude)
    {
        Guard.IsNotNullOrWhiteSpace(latitude);
        Guard.IsNotNullOrWhiteSpace(longitude);
        return $"[По данным Яндекс Погоды,](https://yandex.ru/pogoda/?lat={latitude}&lon={longitude})";
    }

    public static readonly Lazy<Dictionary<MessageTypes, string>> RussianMessages = new Lazy<Dictionary<MessageTypes, string>>(
        () => new Dictionary<MessageTypes, string>
        {
            { MessageTypes.SingleNoRainPrefix, "{0} {1} {2} отменяется. "},
            { MessageTypes.DoubleNoDifferentRainsPrefix, "{0} {1} отменяется {2}, а {3} {4} отменяется {5}. "},
            { MessageTypes.DoubleNoSameRainsSameDatePrefix, "{0} и {1} {2} отменяется {3}. "},
            { MessageTypes.DoubleNoSameRainsPrefix, "{0} {1} и {2} {3} отменяется {4}. "},
            { MessageTypes.WeatherTemplateForSameConditions, " {0} {1} и {2} {3} ожидается {4}." },
            { MessageTypes.WeatherTemplateForDifferentConditions, " {0} ожидается {1} {2}, а {3} {4} - {5}." },
            { MessageTypes.WeatherTemplateForOneRecord, " {0} {1} ожидается {2}." }
        }
        );

    public static readonly Lazy<Dictionary<string, string>> RussianConditions = new Lazy<Dictionary<string, string>>(
       () => new Dictionary<string, string>
       {
           { "light-rain", "Небольшой дождь" },
           { "rain", "Дождь" },
           { "heavy-rain", "Сильный дождь" },
           { "showers", "Ливень" },
           { "wet-snow", "Дождь со снегом" },
           { "light-snow", "Небольшой снег" },
           { "snow", "Снег" },
           { "snow-showers", "Снегопад" },
           { "hail", "Град" },
           { "thunderstorm ", "Гроза" },
           { "thunderstorm-with-rain", "Дождь с грозой" },
           { "thunderstorm-with-hail", "Гроза с градом" },
           { "overcast", "Пасмурная погода"},
           { "clear", "Ясная погода" },
           { "cloudy", "Облачная погода" },
           { "partly-cloudy", "Частичная облачность" } 
       });

    public static readonly Lazy<Dictionary<DayTime, string>> RussianDayTimes = new Lazy<Dictionary<DayTime, string>>(
       () => new Dictionary<DayTime, string>
       {
           { DayTime.Morning, "Утром" },
           { DayTime.Day, "Днём" },
           { DayTime.Evening, "Вечером" },
           { DayTime.Night, "Ночью" }
       });
}
