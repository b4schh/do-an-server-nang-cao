FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution và project files
COPY *.sln .
COPY src/DoAn.Presentation.Api/*.csproj ./src/DoAn.Presentation.Api/
COPY src/DoAn.Core.Application/*.csproj ./src/DoAn.Core.Application/
COPY src/DoAn.Core.Domain/*.csproj ./src/DoAn.Core.Domain/
COPY src/DoAn.Infrastructure/*.csproj ./src/DoAn.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy toàn bộ source code
COPY src/ ./src/

# Build và publish API project
RUN dotnet publish src/DoAn.Presentation.Api/DoAn.Presentation.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "DoAn.Presentation.Api.dll"]