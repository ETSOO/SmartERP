# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS with-node
RUN apt-get update \
	&& apt-get -y install curl \
	&& curl -sL https://deb.nodesource.com/setup_24.x | bash \
	&& apt-get -y install nodejs

# This stage is used to build the service project
FROM with-node AS build
WORKDIR /src
COPY ["Platform/Platform.Server/Platform.Server.csproj", "Platform/Platform.Server/"]
COPY ["WebTemplates/WebTemplates.csproj", "WebTemplates/"]
COPY ["PlatformShared/PlatformShared.csproj", "PlatformShared/"]
COPY ["Platform/platform.client/platform.client.esproj", "Platform/platform.client/"]
RUN dotnet restore "./Platform/Platform.Server/Platform.Server.csproj"
COPY . .
WORKDIR "/src/Platform/Platform.Server"
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./Platform.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Platform.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Platform.Server.dll"]