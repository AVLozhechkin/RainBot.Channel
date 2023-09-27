using System.Text.Json.Serialization;

namespace RainBot.Channel.Core.Models.Functions;

public record ServiceToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
