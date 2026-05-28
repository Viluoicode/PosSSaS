# syntax=docker/dockerfile:1

# ---- Build stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching) — copy only project files.
COPY PosSSaS.sln ./
COPY src/PosSSaS.Domain/PosSSaS.Domain.csproj           src/PosSSaS.Domain/
COPY src/PosSSaS.Application/PosSSaS.Application.csproj  src/PosSSaS.Application/
COPY src/PosSSaS.Infrastructure/PosSSaS.Infrastructure.csproj src/PosSSaS.Infrastructure/
COPY src/PosSSaS.API/PosSSaS.API.csproj                 src/PosSSaS.API/
RUN dotnet restore PosSSaS.sln

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/PosSSaS.API/PosSSaS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Kestrel listens on 8080 inside the container.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "PosSSaS.API.dll"]
