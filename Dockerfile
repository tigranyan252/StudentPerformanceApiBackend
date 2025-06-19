# СТАДИЯ 1: build (Сборка приложения)
# Используем официальный образ .NET SDK 8.0 для сборки нашего приложения.
# Этот образ содержит все необходимые инструменты (.NET CLI, компиляторы).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Устанавливаем рабочую директорию внутри контейнера.
# Все последующие команды будут выполняться относительно этой директории.
WORKDIR /src

# Копируем файл .csproj вашего проекта API.
# Это важно для Docker кэша: если вы меняете только код C#, а не зависимости NuGet,
# Docker не будет заново выполнять dotnet restore, что ускоряет сборку.
# "StudentPerformance.Api/StudentPerformance.Api.csproj" - это ПУТЬ ВНУТРИ РЕПОЗИТОРИЯ
# относительно места, откуда запускается Docker (обычно корень репозитория).
# Если ваш Dockerfile находится прямо рядом с .csproj, то путь будет просто "StudentPerformance.Api.csproj".
# Если ваш репозиторий устроен так:
# MyRepo/
#   MyBackendApi/
#     MyBackendApi.csproj
#     Dockerfile  <-- Dockerfile здесь
# Тогда путь будет: "MyBackendApi.csproj"
# Но если ваш репозиторий содержит несколько проектов:
# MyRepo/
#   Src/
#     MyBackendApi/
#       MyBackendApi.csproj
#     MyOtherProject/
#   Dockerfile <-- Dockerfile здесь (в корне репозитория)
# Тогда путь будет: "Src/MyBackendApi/MyBackendApi.csproj"
#
# В вашем случае, если Dockerfile находится в `StudentPerformance.Api/`,
# и `StudentPerformance.Api.csproj` находится там же,
# то строка `COPY ["StudentPerformance.Api/StudentPerformance.Api.csproj", "StudentPerformance.Api/"]`
# должна быть просто `COPY ["StudentPerformance.Api.csproj", "."]`
#
# Давайте предположим, что `Dockerfile` находится в `StudentPerformance.Api` рядом с `.csproj`
# И `dotnet restore` должен быть запущен в контексте этой папки.

# ИСПРАВЛЕННЫЙ БЛОК КОПИРОВАНИЯ И ВОССТАНОВЛЕНИЯ ДЛЯ ВАШЕЙ СТРУКТУРЫ
# Предполагаем, что Dockerfile находится в StudentPerformance.Api
WORKDIR /src/StudentPerformance.Api # Меняем рабочую директорию на папку проекта
COPY ["StudentPerformance.Api.csproj", "./"] # Копируем только .csproj в текущую рабочую директорию
RUN dotnet restore "StudentPerformance.Api.csproj" # Восстанавливаем зависимости

# Копируем остальной код приложения в текущую рабочую директорию.
COPY . .

# Собираем приложение в режиме "Release" и помещаем его в "/app/build"
RUN dotnet build "StudentPerformance.Api.csproj" -c Release -o /app/build

# СТАДИЯ 2: publish (Публикация приложения)
# Используем результаты стадии "build" для публикации приложения.
# Результат публикации - это готовое к развертыванию приложение без исходного кода.
FROM build AS publish
# Публикуем приложение в режиме "Release" в папку "/app/publish"
# /p:UseAppHost=false - важен для Heroku/Railway, чтобы избежать проблем с запуском на некоторых хостингах.
RUN dotnet publish "StudentPerformance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# СТАДИЯ 3: final (Конечный образ для запуска)
# Используем официальный образ .NET ASPNET runtime (только среда выполнения, без SDK).
# Этот образ намного меньше, что делает конечный контейнер компактнее.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Устанавливаем рабочую директорию для конечного приложения.
WORKDIR /app
# Копируем опубликованные файлы из стадии "publish" в конечный образ.
COPY --from=publish /app/publish .

# Переменные среды:
# ENV ASPNETCORE_URLS=http://+:8080 - Указывает ASP.NET Core слушать запросы на порту 8080.
# Heroku/Railway обычно перенаправляют внешний трафик на порт 8080 внутри контейнера.
ENV ASPNETCORE_URLS=http://+:8080
# EXPOSE 8080 - Информирует Docker, что контейнер слушает на порту 8080.
# Это декларация, не открывает порт наружу сам по себе.
EXPOSE 8080

# ENTRYPOINT - Указывает команду, которая будет выполняться при запуске контейнера.
# "dotnet StudentPerformance.Api.dll" - запускает ваше скомпилированное API.
ENTRYPOINT ["dotnet", "StudentPerformance.Api.dll"]