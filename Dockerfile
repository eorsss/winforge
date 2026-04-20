# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["WinForge.sln", "./"]
COPY ["WinForge.API/WinForge.API.csproj", "WinForge.API/"]
COPY ["WinForge.Core/WinForge.Core.csproj", "WinForge.Core/"]
COPY ["WinForge.Infrastructure/WinForge.Infrastructure.csproj", "WinForge.Infrastructure/"]
COPY ["WinForge.Shared/WinForge.Shared.csproj", "WinForge.Shared/"]

# Restore dependencies
RUN dotnet restore

# Copy entire source
COPY . .

# Build and publish API
WORKDIR "/src/WinForge.API"
RUN dotnet publish "WinForge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render uses PORT env var, but ASP.NET defaults to 8080 in .NET 8+)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WinForge.API.dll"]
