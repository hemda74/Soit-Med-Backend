# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5117

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SoitMed.csproj", "./"]
RUN dotnet restore "SoitMed.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "SoitMed.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoitMed.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure the app to listen on all interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:5117
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "SoitMed.dll"]



