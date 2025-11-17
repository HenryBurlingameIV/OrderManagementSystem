FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Shared/OrderManagementSystem.Shared.csproj", "OrderManagementSystem.Shared/"]
COPY ["./AuthService/AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["./AuthService/AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["./AuthService/AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["./AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]
RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"


COPY . .
WORKDIR "/app/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "AuthService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8081

COPY --from=publish /app/publish .
COPY --from=build /app/AuthService.Infrastructure/Migrations ./Migrations/

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
