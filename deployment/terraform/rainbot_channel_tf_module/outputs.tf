output "yandex_database_endpoint" {
  value = yandex_ydb_database_serverless.rainbot_channel_db.ydb_api_endpoint
}

output "yandex_database_path" {
  value = yandex_ydb_database_serverless.rainbot_channel_db.database_path
}