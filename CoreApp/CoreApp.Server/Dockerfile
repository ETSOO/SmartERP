# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
# USER $APP_UID
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    tzdata

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

# This stage is used to build the service project
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoreApp/coreapp.client/nuget.config", "CoreApp/coreapp.client/"]
COPY ["CoreApp/CoreApp.Server/CoreApp.Server.csproj", "CoreApp/CoreApp.Server/"]
COPY ["PlatformShared/PlatformShared.csproj", "PlatformShared/"]
COPY ["CoreApp/coreapp.client/coreapp.client.esproj", "CoreApp/coreapp.client/"]
RUN dotnet restore "./CoreApp/CoreApp.Server/CoreApp.Server.csproj"
COPY . .
WORKDIR "/src/CoreApp/CoreApp.Server"
RUN dotnet build "./CoreApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreApp.Server.dll"]