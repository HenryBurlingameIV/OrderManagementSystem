FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./OrderNotificationService.Api/OrderNotificationService.Api.csproj", "OrderNotificationService.Api/"]
COPY ["./OrderNotificationService.Application/OrderNotificationService.Application.csproj", "OrderNotificationService.Application/"]
COPY ["./OrderNotificationService.Domain/OrderNotificationService.Domain.csproj", "OrderNotificationService.Domain/"]
COPY ["./OrderNotificationService.Infrastructure/OrderNotificationService.Infrastructure.csproj", "OrderNotificationService.Infrastructure/"]
RUN dotnet restore "OrderNotificationService.Api/OrderNotificationService.Api.csproj"


COPY . .
WORKDIR "/app/OrderNotificationService.Api"
RUN dotnet build "OrderNotificationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "OrderNotificationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8083

COPY --from=publish /app/publish .
COPY --from=build /app/OrderNotificationService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "OrderNotificationService.Api.dll"]