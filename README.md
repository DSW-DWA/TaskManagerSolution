## **Task Manager Solution**

Микросервисное приложение для управления задачами с централизованным логированием, асинхронной обработкой сообщений и трассировкой через **OpenTelemetry**.
Проект включает три основных сервиса и вспомогательные инфраструктурные компоненты, развёртываемые с помощью Docker Compose.

---

## **Архитектура проекта**

```
/TaskManagerSolution
│
├── TaskAPI/                 → основной API для управления задачами
│   ├── TaskAPI/
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── ...
│
├── LogService/              → REST-сервис для приёма и хранения логов
│   ├── LogService/
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── ...
│
├── LogServiceConsumer/      → обработчик сообщений из RabbitMQ
│   ├── LogServiceConsumer/
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── ...
│
├── docker-compose.yml       → файл для запуска всей инфраструктуры
├── otel-config.yaml         → конфигурация OpenTelemetry Collector
└── README.md                → этот файл
```

---

## **Основные возможности**

### TaskAPI

* CRUD API для управления задачами.
* Использует **Entity Framework Core (Code First)** и **PostgreSQL**.
* Отправляет логи:

  * в **LogService** по HTTP;
  * в **RabbitMQ** — для асинхронной обработки LogServiceConsumer.
* Поддерживает трассировку через **OpenTelemetry** (OTLP).

### LogService

* REST API для приёма логов по `/api/logs`.
* Сохраняет и выводит содержимое `Payload` в консоль.
* Может быть интегрирован с внешними системами мониторинга.
* Поддерживает **OpenTelemetry трассировку** для входящих запросов.

### LogServiceConsumer

* Подписан на очередь RabbitMQ.
* Асинхронно обрабатывает полученные сообщения.
* Добавляет `Activity` (span) для каждой операции.
* Отправляет метрики и трассировки в **OpenTelemetry Collector**.

---

## **Инфраструктура**

| Компонент                   | Назначение                         |
| --------------------------- | ---------------------------------- |
| **PostgreSQL**              | база данных для `TaskAPI`          |
| **RabbitMQ (3-management)** | брокер сообщений с веб-интерфейсом |
| **OpenTelemetry Collector** | агрегатор трассировок и метрик     |
| **Jaeger**                  | просмотр трассировок (UI)          |
| **TaskAPI**                 | основной REST API                  |
| **LogService**              | HTTP сервис логирования            |
| **LogServiceConsumer**      | обработчик сообщений из RabbitMQ   |

---

## **Технологии**

| Область           | Используемая технология           |
| ----------------- | --------------------------------- |
| Платформа         | .NET 8 (ASP.NET Core Minimal API) |
| ORM               | Entity Framework Core             |
| База данных       | PostgreSQL                        |
| Очередь сообщений | RabbitMQ                          |
| Логирование       | Console / HTTP / RabbitMQ         |
| Трассировка       | OpenTelemetry + Jaeger            |
| Контейнеризация   | Docker, Docker Compose            |

---

## **Запуск проекта**

### Требования

Перед запуском убедитесь, что установлены:

* [Docker Desktop](https://www.docker.com/products/docker-desktop)
* [Docker Compose v2+](https://docs.docker.com/compose/)

---

### Сборка и запуск

Из корня проекта:

```bash
docker compose up --build
```

* При первом запуске загружаются образы и выполняется инициализация базы данных.
* Все миграции применяются автоматически при старте `TaskAPI`.

---

## **Доступ к сервисам**

| Сервис                 | URL / Порт                                                     | Назначение                                |
| ---------------------- | -------------------------------------------------------------- | ----------------------------------------- |
| **TaskAPI**            | [http://localhost:5000/swagger](http://localhost:5000/swagger) | Основное REST API                         |
| **LogService**         | [http://localhost:5070/swagger](http://localhost:5070/swagger) | REST API логирования                      |
| **RabbitMQ**           | [http://localhost:15672](http://localhost:15672)               | Панель RabbitMQ                           |
| **Jaeger (UI)**        | [http://localhost:16686](http://localhost:16686)               | Просмотр трассировок OpenTelemetry        |
| **PostgreSQL**         | `localhost:5432`                                               | Хранилище данных                          |
| **LogServiceConsumer** | —                                                              | Работает в фоне, слушает очередь RabbitMQ |

---

## **Подключение к базе данных**

Параметры подключения:

```
Host: localhost
Port: 5432
Database: taskdb
Username: postgres
Password: postgres
```

Пример подключения через `psql`:

```bash
psql -h localhost -U postgres -d taskdb
```

---

## **OpenTelemetry и Jaeger**

Система трассировки собрана на базе:

* **OpenTelemetry Collector** (`otel-collector:4317`)
* **Jaeger** для визуализации (`http://localhost:16686`)

После выполнения HTTP-запросов к `TaskAPI` или при поступлении сообщений в `LogServiceConsumer`,
трассы можно просматривать в Jaeger UI → поле **Service** будет содержать:

* `TaskAPI`
* `LogService`
* `LogServiceConsumer`

---

## **Типовой сценарий работы**

1. Запустить инфраструктуру:

   ```bash
   docker compose up --build
   ```

2. Перейти к [Swagger UI](http://localhost:5000/swagger).

3. Создать новую задачу через `POST /api/tasks`.

4. Проверить:

   * Лог события появился в **LogService**.
   * В **RabbitMQ** создано сообщение.
   * **LogServiceConsumer** вывел сообщение в консоль.
   * В **Jaeger** появилась трассировка.

---

## **Компоненты docker-compose.yml**

* `postgres` — база данных PostgreSQL
* `rabbitmq` — брокер сообщений RabbitMQ
* `otel-collector` — сборщик и маршрутизатор телеметрии
* `jaeger` — визуализация трассировок
* `logservice` — REST-сервис логирования
* `logserviceconsumer` — обработчик сообщений RabbitMQ
* `taskapi` — основной API

---

## **Остановка и очистка окружения**

```bash
docker compose down -v
```

> ⚠️ Параметр `-v` удаляет также volume с данными PostgreSQL.