FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./OrderProcessingService.Api/OrderProcessingService.Api.csproj", "OrderProcessingService.Api/"]
COPY ["./OrderProcessingService.Application/OrderService.Application.csproj", "OrderProcessingService.Application/"]
COPY ["./OrderProcessingService.Domain/OrderProcessingService.Domain.csproj", "OrderProcessingService.Domain/"]
COPY ["./OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
RUN dotnet restore "OrderService.Api/OrderService.Api.csproj"


COPY . .
WORKDIR "/app/OrderService.Api"
RUN dotnet build "OrderProcessingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "OrderProcessingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8082

COPY --from=publish /app/publish .
COPY --from=build /app/OrderProcessingService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "OrderService.Api.dll"]

