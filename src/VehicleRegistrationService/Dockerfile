FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build-env
WORKDIR /app

# Copy necessary files and restore as distinct layer
COPY VehicleRegistrationService.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out VehicleRegistrationService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim
COPY --from=build-env /app/out .

# Expose ports
EXPOSE 6002/tcp
ENV ASPNETCORE_URLS http://*:6002

# Start
ENTRYPOINT ["dotnet", "VehicleRegistrationService.dll"]
