
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["./OrderManagementSystem.Gateway/OrderManagementSystem.Gateway.csproj", "OrderManagementSystem.Gateway/"]
RUN dotnet restore "OrderManagementSystem.Gateway/OrderManagementSystem.Gateway.csproj"


COPY . .
WORKDIR "/app/OrderManagementSystem.Gateway"
RUN dotnet build "OrderManagementSystem.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM build AS publish
RUN dotnet publish "OrderManagementSystem.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8089

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OrderManagementSystem.Gateway.dll"]