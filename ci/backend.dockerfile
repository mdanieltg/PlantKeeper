# Use Microsoft .NET 10 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PlantKeeperAPI/src/PlantKeeperAPI.csproj", "PlantKeeperAPI/src/"]
RUN dotnet restore "PlantKeeperAPI/src/PlantKeeperAPI.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/PlantKeeperAPI/src"

# Build and publish the application
RUN dotnet build "PlantKeeperAPI.csproj" -c Release -o /app/build
RUN dotnet publish "PlantKeeperAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the .NET 10 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PlantKeeperAPI.dll"]
