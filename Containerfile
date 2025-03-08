FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /usr/src/anna
COPY . .

RUN dotnet tool restore
RUN dotnet restore

RUN dotnet publish --no-restore --configuration Release Anna.Api

FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt update && apt install -y python3

WORKDIR /app
COPY --from=build /usr/src/anna/Anna.Api/bin/Release/net8.0/publish .
COPY container-entrypoint.sh .
COPY scripts/set_cors_hosts.py .

RUN mkdir -pv /data
ENV ANNA_STORAGE_ROOT_DIR="/data/packages"

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/bin/bash", "container-entrypoint.sh"]
