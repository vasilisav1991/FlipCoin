# Multi-stage build: the SDK image compiles the Blazor WebAssembly app to
# static files; the final image is plain nginx serving those files (WASM runs
# in the browser — there is no .NET on this server). Build context is the
# repository root.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Flipcoin.Client/Flipcoin.Client.csproj src/Flipcoin.Client/
RUN dotnet restore src/Flipcoin.Client/Flipcoin.Client.csproj

COPY src/Flipcoin.Client/ src/Flipcoin.Client/
RUN dotnet publish src/Flipcoin.Client/Flipcoin.Client.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
COPY src/Flipcoin.Client/nginx.conf /etc/nginx/conf.d/default.conf
# The published site (index.html, _framework/, css/, appsettings.json) lands
# under wwwroot in the publish output.
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
