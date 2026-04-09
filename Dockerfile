# Base stage for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["PromptHQ.API/PromptHQ.API.csproj", "PromptHQ.API/"]
COPY ["PromptHQ.Application/PromptHQ.Application.csproj", "PromptHQ.Application/"]
COPY ["PromptHQ.Infrastructure/PromptHQ.Infrastructure.csproj", "PromptHQ.Infrastructure/"]
COPY ["PromptHQ.AI/PromptHQ.AI.csproj", "PromptHQ.AI/"]
COPY ["PromptHQ.Communication/PromptHQ.Communication.csproj", "PromptHQ.Communication/"]
COPY ["PromptHQ.Domain/PromptHQ.Domain.csproj", "PromptHQ.Domain/"]

RUN dotnet restore "PromptHQ.API/PromptHQ.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/PromptHQ.API"
RUN dotnet build "PromptHQ.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PromptHQ.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PromptHQ.API.dll"]
