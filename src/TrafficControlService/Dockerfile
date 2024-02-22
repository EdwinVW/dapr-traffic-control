FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /build

# Copy necessary files and restore as distinct layer
COPY TrafficControlService.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out TrafficControlService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /build/out .

# Expose ports
EXPOSE 6000/tcp
ENV ASPNETCORE_URLS http://*:6000

# Start
ENTRYPOINT ["dotnet", "TrafficControlService.dll"]
