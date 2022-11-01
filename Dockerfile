FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /ACBot

COPY . ./
RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /ACBot
COPY --from=build-env /ACBot/out .
ENTRYPOINT ["dotnet", "ACBot.Discord.dll"]
