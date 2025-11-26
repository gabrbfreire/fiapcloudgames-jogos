FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FiapCloudGames.Jogos.sln ./
COPY src/FiapCloudGames.Jogos.API/FiapCloudGames.Jogos.API.csproj src/FiapCloudGames.Jogos.API/
COPY src/FiapCloudGames.Jogos.Core/FiapCloudGames.Jogos.Core.csproj src/FiapCloudGames.Jogos.Core/
COPY src/FiapCloudGames.Jogos.Infra/FiapCloudGames.Jogos.Infra.csproj src/FiapCloudGames.Jogos.Infra/
COPY src/FiapCloudGames.Jogos.Test/FiapCloudGames.Jogos.Test.csproj src/FiapCloudGames.Jogos.Test/

RUN dotnet restore src/FiapCloudGames.Jogos.API/FiapCloudGames.Jogos.API.csproj

COPY . .

WORKDIR /src/src/FiapCloudGames.Jogos.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "FiapCloudGames.Jogos.API.dll"]
