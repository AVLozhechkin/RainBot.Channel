locals {
  sqs_endpoint_region = "ru-central1"
}

resource "yandex_function" "yandex-weather-fetcher" {
  name               = "yandex-weather-fetcher"
  user_hash          = "${timestamp()}}"
  description        = "Function that fetches forecasts from Yandex.Weather"
  runtime            = "dotnet6"
  entrypoint         = "RainBot.Channel.YandexWeatherFetcher.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YANDEX_WEATHER_API_KEY" : var.yandex_weather_api_key,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "FORECAST_HANDLER_QUEUE" : yandex_message_queue.forecast-handler-queue.id,
    "LATITUDE": var.latitude,
    "LONGITUDE": var.longitude
  }
  content {
    zip_filename = "../zips/RainBot.Channel.YandexWeatherFetcher.zip"
  }
}

resource "yandex_function" "forecast-handler" {
  name               = "forecast-handler"
  user_hash          = "${timestamp()}}"
  description        = "Function that parses weather forecasts, stores it into database and sends notifications if it detects rain."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.Channel.ForecastHandler.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YDB_DATABASE" : yandex_ydb_database_serverless.rainbot_channel_db.database_path,
    "TG_TOKEN" : var.telegram_bot_token,
    "TG_CHANNEL_ID": var.telegram_channel_id,
    "LATITUDE": var.latitude,
    "LONGITUDE": var.longitude
  }
  content {
    zip_filename = "../zips/RainBot.Channel.ForecastHandler.zip"
  }
}