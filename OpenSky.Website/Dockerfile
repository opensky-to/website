FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY /OpenSky.Website/OpenSky.Website.csproj .
RUN dotnet restore "OpenSky.Website.csproj"
COPY /OpenSky.Website/. .
RUN dotnet build "OpenSky.Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenSky.Website.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY /OpenSky.Website/nginx.conf /etc/nginx/nginx.conf