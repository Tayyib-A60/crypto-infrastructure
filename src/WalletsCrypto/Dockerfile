#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 4000
EXPOSE 4001

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WalletsCrypto/WalletsCrypto.csproj", "WalletsCrypto/"]
COPY ["WalletsCrypto.Application/WalletsCrypto.Application.csproj", "WalletsCrypto.Application/"]
COPY ["WalletsCrypto.Infrastructure/WalletsCrypto.Infrastructure.csproj", "WalletsCrypto.Infrastructure/"]
COPY ["WalletsCrypto.Domain/WalletsCrypto.Domain.csproj", "WalletsCrypto.Domain/"]
COPY ["WalletsCrypto.ReadModel/WalletsCrypto.ReadModel.csproj", "WalletsCrypto.ReadModel/"]
COPY ["WalletsCrypto.Domain.EventStore/WalletsCrypto.Domain.EventStore.csproj", "WalletsCrypto.Domain.EventStore/"]

RUN dotnet restore -s https://nuget.wallets.africa/nuget -s https://api.nuget.org/v3/index.json "WalletsCrypto/WalletsCrypto.csproj"
COPY . .
WORKDIR "/src/WalletsCrypto"
RUN dotnet build "WalletsCrypto.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletsCrypto.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletsCrypto.dll"]
