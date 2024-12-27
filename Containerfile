FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /usr/src/anna
COPY . .

RUN mkdir -pv /data

ENV ANNA_INDEX_DB_PATH="/data/index.db"

RUN dotnet tool restore
RUN dotnet restore

RUN dotnet ef migrations script --project Anna.Index/Anna.Index.csproj --output migrations.sql

RUN dotnet publish --no-restore --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt update
RUN apt install -y --no-install-recommends sqlite3

WORKDIR /app
COPY --from=build /usr/src/anna/Anna.Api/bin/Release/net8.0/publish .
COPY --from=build /usr/src/anna/container_entrypoint.sh .
COPY --from=build /usr/src/anna/migrations.sql .

ENTRYPOINT ["/bin/bash", "container_entrypoint.sh"]
