FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /build

# Copy necessary files and restore as distinct layer
COPY Simulation.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out Simulation.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /build/out .

# Start
ENTRYPOINT ["dotnet", "Simulation.dll"]
