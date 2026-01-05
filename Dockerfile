FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution và project files
COPY *.sln .
COPY src/FootballField.Api/*.csproj ./src/FootballField.Api/
COPY src/FootballField.Application/*.csproj ./src/FootballField.Application/
COPY src/FootballField.Domain/*.csproj ./src/FootballField.Domain/
COPY src/FootballField.Infrastructure/*.csproj ./src/FootballField.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy toàn bộ source code
COPY src/ ./src/

# Build và publish API project
RUN dotnet publish src/FootballField.Api/FootballField.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "FootballField.Api.dll"]