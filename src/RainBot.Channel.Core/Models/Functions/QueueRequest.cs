﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RainBot.Channel.Core.Models.Functions;

public record QueueRequest
{
    [JsonPropertyName("messages")]
    public IReadOnlyList<Message> Messages { get; set; }
}

public record Message
{
    [JsonPropertyName("details")]
    public Details Details { get; set; }
}

public record Details
{
    [JsonPropertyName("message")]
    public MessageBody Message { get; set; }
}

public record MessageBody
{
    [JsonPropertyName("body")]
    public string Body { get; set; }
}
