FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrderManagementApi/OrderManagementApi.csproj", "OrderManagementApi/"]
COPY ["MessageContracts/MessageContracts.csproj", "MessageContracts/"]
COPY ["RabbitMQ/RabbitMQ.csproj", "RabbitMQ/"]

RUN dotnet restore "OrderManagementApi/OrderManagementApi.csproj"

COPY . .
WORKDIR "/src/OrderManagementApi"
RUN dotnet publish "OrderManagementApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderManagementApi.dll"]
