FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SNEL.WalletPlatform.Presentation/SNEL.WalletPlatform.Presentation.csproj", "SNEL.WalletPlatform.Presentation/"]
COPY ["SNEL.WalletPlatform.Application/SNEL.WalletPlatform.Application.csproj", "SNEL.WalletPlatform.Application/"]
COPY ["SNEL.WalletPlatform.Domain/SNEL.WalletPlatform.Domain.csproj", "SNEL.WalletPlatform.Domain/"]
COPY ["SNEL.WalletPlatform.Infrastructure/SNEL.WalletPlatform.Infrastructure.csproj", "SNEL.WalletPlatform.Infrastructure/"]
RUN dotnet restore "SNEL.WalletPlatform.Presentation/SNEL.WalletPlatform.Presentation.csproj"
COPY . .
WORKDIR "/src/SNEL.WalletPlatform.Presentation"
RUN dotnet build "SNEL.WalletPlatform.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SNEL.WalletPlatform.Presentation.csproj" -c Release -o /app/publish
    
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SNEL.WalletPlatform.Presentation.dll"]