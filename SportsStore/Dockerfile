FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore/SportsStore.csproj", "SportsStore/"]
COPY ["SportsStore.Tests/SportsStore.Tests.csproj", "SportsStore.Tests/"]
RUN dotnet restore "SportsStore/SportsStore.csproj"
COPY . .
WORKDIR "/src/SportsStore"
RUN dotnet publish "SportsStore.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SportsStore.dll"]
