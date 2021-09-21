#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0.0-rc.1-bullseye-slim-amd64 AS base
RUN apt-get update && apt-get install -y libc6-dev libgdiplus
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BotNet/BotNet.csproj", "BotNet/"]
RUN dotnet restore "BotNet/BotNet.csproj"
COPY . .
WORKDIR "/src/BotNet"
RUN dotnet build "BotNet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BotNet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotNet.dll"]