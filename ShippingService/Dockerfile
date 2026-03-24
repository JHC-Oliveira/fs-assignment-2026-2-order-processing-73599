FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ShippingService/ShippingService.csproj", "ShippingService/"]
COPY ["MessageContracts/MessageContracts.csproj", "MessageContracts/"]
COPY ["RabbitMQ/RabbitMQ.csproj", "RabbitMQ/"]

RUN dotnet restore "ShippingService/ShippingService.csproj"

COPY . .
WORKDIR "/src/ShippingService"
RUN dotnet publish "ShippingService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShippingService.dll"]
