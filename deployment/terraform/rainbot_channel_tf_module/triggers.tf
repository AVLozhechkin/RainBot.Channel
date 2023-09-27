resource "yandex_function_trigger" "forecast-handler-trigger" {

  name        = "forecast-handler-trigger"
  description = "Calls forecast-handler function when there is new message in forecast-handler-queue."

  message_queue {
    queue_id           = yandex_message_queue.forecast-handler-queue.arn
    service_account_id = yandex_iam_service_account.service_account.id
    batch_size         = "1"
    batch_cutoff       = "10"
  }

  function {
    id                 = yandex_function.forecast-handler.id
    service_account_id = yandex_iam_service_account.service_account.id
  }

}

resource "yandex_function_trigger" "weather-fetcher-trigger" {
  name        = "weather-fetcher-trigger"
  description = "Fetches weather forecasts from Yandex every hour."
  timer {
    cron_expression = "0 * ? * * *"
  }
  function {
    id                 = yandex_function.yandex-weather-fetcher.id
    service_account_id = yandex_iam_service_account.service_account.id
  }
}