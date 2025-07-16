FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HotstarScraper/HotstarScraper.csproj", "HotstarScraper/"]
RUN dotnet restore "./HotstarScraper/HotstarScraper.csproj"
COPY . .
WORKDIR "/src/HotstarScraper"
RUN dotnet build "./HotstarScraper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HotstarScraper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotstarScraper.dll"] 