FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./OrderProcessingService/OrderProcessingService.Api/OrderProcessingService.Api.csproj", "OrderProcessingService/OrderProcessingService.Api/"]
COPY ["./OrderProcessingService/OrderProcessingService.Application/OrderProcessingService.Application.csproj", "OrderProcessingService/OrderProcessingService.Application/"]
COPY ["./OrderProcessingService/OrderProcessingService.Domain/OrderProcessingService.Domain.csproj", "OrderProcessingService/OrderProcessingService.Domain/"]
COPY ["./OrderProcessingService/OrderProcessingService.Infrastructure/OrderProcessingService.Infrastructure.csproj", "OrderProcessingService/OrderProcessingService.Infrastructure/"]
RUN dotnet restore "OrderProcessingService/OrderProcessingService.Api/OrderProcessingService.Api.csproj"
COPY . .

WORKDIR "/app/OrderProcessingService/OrderProcessingService.Api"
RUN dotnet build "OrderProcessingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "OrderProcessingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8082

COPY --from=publish /app/publish .
COPY --from=build /app/OrderProcessingService/OrderProcessingService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "OrderProcessingService.Api.dll"]

