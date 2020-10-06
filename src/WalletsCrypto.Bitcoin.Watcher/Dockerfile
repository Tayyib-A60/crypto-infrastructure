#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WalletsCrypto.Bitcoin.Watcher/WalletsCrypto.Bitcoin.Watcher.csproj", "WalletsCrypto.Bitcoin.Watcher/"]
RUN dotnet restore -s https://nuget.wallets.africa/nuget -s https://api.nuget.org/v3/index.json "WalletsCrypto.Bitcoin.Watcher/WalletsCrypto.Bitcoin.Watcher.csproj"
COPY . .
WORKDIR "/src/WalletsCrypto.Bitcoin.Watcher"
RUN dotnet build "WalletsCrypto.Bitcoin.Watcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletsCrypto.Bitcoin.Watcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletsCrypto.Bitcoin.Watcher.dll"]