# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только текущий csproj (из текущей папки)
COPY *.csproj ./
RUN dotnet restore

# Теперь копируем всё остальное из текущей директории
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# 2️⃣ Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 5070
ENV ASPNETCORE_URLS=http://+:5070

ENTRYPOINT ["dotnet", "LogService.dll"]
