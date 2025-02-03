FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /usr/src/anna
COPY . .

RUN dotnet tool restore
RUN dotnet restore

RUN dotnet publish --no-restore --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /usr/src/anna/Anna.Api/bin/Release/net8.0/publish .

RUN mkdir -pv /data
ENV ANNA_STORAGE_ROOT_DIR="/data/packages"

ENTRYPOINT ["dotnet", "Anna.Api.dll"]
