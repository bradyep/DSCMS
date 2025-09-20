# Use multi-stage build for smaller final image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY ["DSCMS.csproj", "."]

# Restore dependencies
RUN dotnet restore "DSCMS.csproj"

# Copy source code
COPY . .

# Build and publish the application
RUN dotnet publish "DSCMS.csproj" -c Release -o /app/publish --no-restore

# Use runtime image for final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install sqlite3 for database operations and debugging
RUN apt-get update && apt-get install -y \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Create non-root user for security
RUN groupadd -r dscms && useradd -r -g dscms dscms

# Create app directory and set permissions
WORKDIR /app
RUN chown -R dscms:dscms /app

# Create directory for SQLite database with proper permissions
RUN mkdir -p /dscms-data && chown -R dscms:dscms /dscms-data

# Copy published application
COPY --from=build /app/publish .

# Switch to non-root user
USER dscms

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/ || exit 1

# Run the application
ENTRYPOINT ["dotnet", "DSCMS.dll"]
