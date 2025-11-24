FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./OrderService/OrderService.Api/OrderService.Api.csproj", "OrderService/OrderService.Api/"]
COPY ["./OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["./OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["./OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
RUN dotnet restore "OrderService/OrderService.Api/OrderService.Api.csproj"


COPY . .
WORKDIR "/app/OrderService/OrderService.Api"
RUN dotnet build "OrderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "OrderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8081

COPY --from=publish /app/publish .
COPY --from=build /app/OrderService/OrderService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
