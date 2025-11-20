# Stage 1: Build the application
# Using .NET 8 SDK to compile the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore NuGet packages
# This creates a "cache layer" - if packages don't change, Docker reuses this step
COPY ControleEstoque.Api.csproj .
RUN dotnet restore "ControleEstoque.Api.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish "ControleEstoque.Api.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
# Using the smaller 'aspnet' image, which only has what's needed to run the API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose port 8080 (default for aspnet:8.0)
EXPOSE 8080

# Set the entrypoint to run the API
# The application will look for ControleEstoque.Api.dll
ENTRYPOINT ["dotnet", "ControleEstoque.Api.dll"]
