
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./CatalogService.Api/CatalogService.Api.csproj", "CatalogService.Api/"]
COPY ["./CatalogService.Application/CatalogService.Application.csproj", "CatalogService.Application/"]
COPY ["./CatalogService.Domain/CatalogService.Domain.csproj", "CatalogService.Domain/"]
COPY ["./CatalogService.Infrastructure/CatalogService.Infrastructure.csproj", "CatalogService.Infrastructure/"]
RUN dotnet restore "CatalogService.Api/CatalogService.Api.csproj"


COPY . .
WORKDIR "/app/CatalogService.Api"
RUN dotnet build "CatalogService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "CatalogService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
COPY --from=build /app/CatalogService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]