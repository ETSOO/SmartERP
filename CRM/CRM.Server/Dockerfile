# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

# When no wwwroot is present, always caused by the client project's SDK version
# <Project Sdk="Microsoft.VisualStudio.JavaScript.Sdk/1.0.3061438">
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CRM/CRM.Server/CRM.Server.csproj", "CRM/CRM.Server/"]
COPY ["PlatformShared/PlatformShared.csproj", "PlatformShared/"]
COPY ["crm.client/crm.client.esproj", "crm.client/"]
RUN dotnet restore "./CRM/CRM.Server/CRM.Server.csproj"
COPY . .
WORKDIR "/src/CRM/CRM.Server"
RUN dotnet build "./CRM.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CRM.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM.Server.dll"]
