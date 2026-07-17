# Multi-stage build: the SDK image (heavy, has the compiler) builds and
# publishes the app; the final image is the much smaller ASP.NET runtime with
# only the published output. Build context is the repository root.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, copying only project files, so Docker's layer cache reuses
# the restored packages when only source code changes.
COPY src/Flipcoin.Domain/Flipcoin.Domain.csproj src/Flipcoin.Domain/
COPY src/Flipcoin.Application/Flipcoin.Application.csproj src/Flipcoin.Application/
COPY src/Flipcoin.Infrastructure/Flipcoin.Infrastructure.csproj src/Flipcoin.Infrastructure/
COPY src/Flipcoin.Api/Flipcoin.Api.csproj src/Flipcoin.Api/
RUN dotnet restore src/Flipcoin.Api/Flipcoin.Api.csproj

COPY src/ src/
RUN dotnet publish src/Flipcoin.Api/Flipcoin.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# The aspnet base image listens on port 8080 by default (ASPNETCORE_HTTP_PORTS).
EXPOSE 8080
ENTRYPOINT ["dotnet", "Flipcoin.Api.dll"]
