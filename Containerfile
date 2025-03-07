FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /usr/src/anna
COPY . .

RUN dotnet tool restore
RUN dotnet restore

# TODO: set CORS url

RUN dotnet publish --no-restore --configuration Release Anna.Api

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /usr/src/anna/Anna.Api/bin/Release/net8.0/publish .
COPY container-entrypoint.sh .

RUN mkdir -pv /data
ENV ANNA_STORAGE_ROOT_DIR="/data/packages"

ENTRYPOINT ["/bin/bash", "container-entrypoint.sh"]
