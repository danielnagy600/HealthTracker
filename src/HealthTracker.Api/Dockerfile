# Több lépcsős build: előbb SDK-val fordítunk, majd a kisebb runtime image-be másolunk.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Először csak a projektfájlokat másoljuk a réteg-gyorsítótár kihasználásához.
COPY src/HealthTracker.Domain/*.csproj         src/HealthTracker.Domain/
COPY src/HealthTracker.Application/*.csproj    src/HealthTracker.Application/
COPY src/HealthTracker.Persistence/*.csproj    src/HealthTracker.Persistence/
COPY src/HealthTracker.Api/*.csproj            src/HealthTracker.Api/
RUN dotnet restore src/HealthTracker.Api/HealthTracker.Api.csproj

# Most jöhet a teljes forrás és a publikálás.
COPY . .
RUN dotnet publish src/HealthTracker.Api/HealthTracker.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HealthTracker.Api.dll"]
