#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WalletsCrypto.Ethereum.Watcher/WalletsCrypto.Ethereum.Watcher.csproj", "WalletsCrypto.Ethereum.Watcher/"]
COPY ["WalletsCrypto.Infrastructure/WalletsCrypto.Infrastructure.csproj", "WalletsCrypto.Infrastructure/"]
COPY ["WalletsCrypto.Common/WalletsCrypto.Common.csproj", "WalletsCrypto.Common/"]
RUN dotnet restore -s https://nuget.wallets.africa/nuget -s https://api.nuget.org/v3/index.json "WalletsCrypto.Ethereum.Watcher/WalletsCrypto.Ethereum.Watcher.csproj"
COPY . .
WORKDIR "/src/WalletsCrypto.Ethereum.Watcher"
RUN dotnet build "WalletsCrypto.Ethereum.Watcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletsCrypto.Ethereum.Watcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletsCrypto.Ethereum.Watcher.dll"]