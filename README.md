# TestWork — отслеживание цен квартир

Web API на .NET 10 для отслеживания цен квартир на `prinzip.su`. Данные хранятся в MSSQL, проверка цен выполняется автоматически каждые 10 минут.

## API

Swagger: `http://localhost:8080/swagger`

### 1. Создание подписки

```http
POST /api/Subscription/createSubscription
```

Метод создаёт подписку на изменение цены квартиры. При создании сервис загружает страницу квартиры, получает актуальное название и цену, после чего сохраняет данные в MSSQL.

Тело запроса:

```json
{
  "apartmentUrl": "https://prinzip.su/flats/example",
  "email": "user@example.com"
}
```

Параметры:

- `apartmentUrl` — ссылка на квартиру с сайта `prinzip.su`;
- `email` — адрес, на который должны отправляться уведомления.

Пример успешного ответа:

```json
{
  "id": 1,
  "apartmentUrl": "https://prinzip.su/flats/example",
  "email": "user@example.com",
  "apartmentName": "Квартира №100",
  "currentPrice": 7500000,
  "createdAtUtc": "2026-08-01T12:00:00Z",
  "lastCheckedAtUtc": "2026-08-01T12:00:00Z"
}
```

Возможные коды ответа:

- `201 Created` — подписка создана;
- `400 Bad Request` — неправильная ссылка или email;
- `409 Conflict` — такая подписка уже существует;
- `502 Bad Gateway` — не удалось загрузить страницу квартиры;
- `422 Unprocessable Entity` — страница загружена, но цена на ней не найдена.

### 2. Получение подписок и цен

```http
GET /api/Subscription/getAllSubscriptions
```

Метод возвращает сохранённые подписки, ссылки на квартиры и последние полученные цены.

Пример запроса:

```http
GET /api/Subscription/getAllSubscriptions
```

Пример ответа:

```json
[
  {
    "id": 1,
    "apartmentUrl": "https://prinzip.su/flats/example",
    "email": "user@example.com",
    "apartmentName": "Квартира №100",
    "currentPrice": 7500000,
    "createdAtUtc": "2026-08-01T12:00:00Z",
    "lastCheckedAtUtc": "2026-08-01T12:10:00Z"
  }
]
```

Метод поддерживает параметр `refresh`:

```http
GET /api/Subscription/getAllSubscriptions?refresh=true
```

Если передать `refresh=true`, перед формированием ответа сервис повторно загрузит страницы всех квартир и обновит цены.

Если параметр не передан или равен `false`, сервис вернёт данные из базы без обращения к сайту.

Код успешного ответа:

```text
200 OK
```

### 3. Ручная проверка изменения цены

```http
PATCH /api/Subscription/updatePrice/{id}
```

Метод предназначен для проверки логики отслеживания цены и отправки уведомлений.

Вместо `{id}` передаётся идентификатор подписки:

```http
PATCH /api/Subscription/updatePrice/1
```

Тело запроса:

```json
{
  "price": 7000000
}
```

Переданная цена используется как предыдущее значение. После этого сервис получает актуальную цену с сайта и сравнивает значения.

Если цены отличаются:

- `priceChanged` получает значение `true`;
- вызывается механизм отправки email;
- в базе сохраняется актуальная цена с сайта.

Пример ответа:

```json
{
  "subscription": {
    "id": 1,
    "apartmentUrl": "https://prinzip.su/flats/example",
    "email": "user@example.com",
    "apartmentName": "Квартира №100",
    "currentPrice": 6800000,
    "createdAtUtc": "2026-08-01T12:00:00Z",
    "lastCheckedAtUtc": "2026-08-01T12:15:00Z"
  },
  "priceChanged": true,
  "notificationSent": false
}
```

Возможные коды ответа:

- `200 OK` — проверка выполнена;
- `400 Bad Request` — цена меньше или равна нулю;
- `404 Not Found` — подписка с указанным `id` не найдена;
- `502 Bad Gateway` — сайт квартиры недоступен;
- `422 Unprocessable Entity` — цена не найдена на странице.

`notificationSent` будет равен `false`, если отправка email отключена в конфигурации.

## Docker

Запуск из папки с `README.md`:

```powershell
docker compose -f TestWork.API/docker-compose.yml up --build -d
```

Контейнеры:

- `testwork-api` — API, порт `8080`;
- `testwork-db` — MSSQL, порт `1433`;
- база данных — `TestWorkPriceTracker`;
- пользователь MSSQL — `sa`;
- пароль по умолчанию — `TestWork_StrongPassword123!`.

Проверка состояния и просмотр логов:

```powershell
docker compose -f TestWork.API/docker-compose.yml ps
docker compose -f TestWork.API/docker-compose.yml logs -f api
```

Остановка:

```powershell
docker compose -f TestWork.API/docker-compose.yml down
```

`down -v` дополнительно удаляет базу данных.

## Параметры

| Параметр | Назначение | По умолчанию |
|---|---|---|
| `MSSQL_SA_PASSWORD` | Пароль MSSQL | `TestWork_StrongPassword123!` |
| `PriceTracking__CheckIntervalMinutes` | Интервал проверки цен | `10` |
| `Email__Enabled` | Отправка email | `false` |
| `Email__Host` | SMTP-сервер | не настроен |
| `Email__Port` | SMTP-порт | `587` |
| `Email__UseSsl` | Использование SSL | `true` |
| `Email__UserName` | Логин SMTP | не настроен |
| `Email__Password` | Пароль SMTP | не настроен |
| `Email__From` | Адрес отправителя | не настроен |

Пароль MSSQL можно переопределить перед запуском:

```powershell
$env:MSSQL_SA_PASSWORD = "New_Strong_Password123!"
docker compose -f TestWork.API/docker-compose.yml up --build -d
```

