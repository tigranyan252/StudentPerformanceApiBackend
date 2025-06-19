# Base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Устанавливаем рабочую директорию внутри контейнера в папку проекта.
WORKDIR /src/StudentPerformance.Api

# Копируем файл .csproj вашего проекта API в текущую рабочую директорию контейнера (./).
# Это StudentPerformance.Api.csproj, так как Dockerfile находится рядом с ним.
COPY ["StudentPerformance.Api.csproj", "./"]
# Восстанавливаем зависимости NuGet.
RUN dotnet restore "StudentPerformance.Api.csproj"

# Копируем остальной исходный код приложения в текущую рабочую директорию.
COPY . .

# Собираем приложение в режиме "Release".
RUN dotnet build "StudentPerformance.Api.csproj" -c Release -o /app/build

# Стадия публикации
FROM build AS publish
# Публикуем приложение.
RUN dotnet publish "StudentPerformance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Конечный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Устанавливаем рабочую директорию в /app.
WORKDIR /app
# Копируем опубликованные файлы из стадии "publish".
COPY --from=publish /app/publish .

# Указываем ASP.NET Core слушать запросы на порту 8080.
ENV ASPNETCORE_URLS=http://+:8080
# Объявляем, что контейнер слушает порт 8080.
EXPOSE 8080

# Запускаем ваше скомпилированное API.
ENTRYPOINT ["dotnet", "StudentPerformance.Api.dll"]
