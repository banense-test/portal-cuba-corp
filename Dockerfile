# Portal Cuba Corp — Dockerfile for containerized deployment
# .NET 10 Razor Pages application (CON-001, CON-002)
# Target: Internal Windows Server (CON-006) — Docker is optional
#
# Build: docker build -t portal-cuba-corp:1.0.0 .
# Run:   docker run -p 5000:5000 portal-cuba-corp:1.0.0

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/PortalCubaCorp.Domain/PortalCubaCorp.Domain.csproj PortalCubaCorp.Domain/
COPY src/PortalCubaCorp.Application/PortalCubaCorp.Application.csproj PortalCubaCorp.Application/
COPY src/PortalCubaCorp.Infrastructure/PortalCubaCorp.Infrastructure.csproj PortalCubaCorp.Infrastructure/
COPY src/PortalCubaCorp/PortalCubaCorp.csproj PortalCubaCorp/

# Regenerate solution and restore
RUN dotnet new sln && \
    find . -name '*.csproj' -exec dotnet sln add {} + && \
    dotnet restore

# Copy source code and build
COPY src/ ./
RUN dotnet build --configuration Release --no-restore

# Publish
RUN dotnet publish PortalCubaCorp/PortalCubaCorp.csproj -c Release -o /app/publish --no-build

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PortalCubaCorp.dll"]