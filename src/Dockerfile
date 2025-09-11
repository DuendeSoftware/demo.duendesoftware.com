FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Duende.IdentityServer.Demo.csproj", "src/"]
RUN dotnet restore "src/Duende.IdentityServer.Demo.csproj" -r linux-x64
COPY . .
WORKDIR "/src/src"
RUN dotnet build "./Duende.IdentityServer.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Duende.IdentityServer.Demo.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore -r linux-x64 --self-contained true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Duende.IdentityServer.Demo.dll"]