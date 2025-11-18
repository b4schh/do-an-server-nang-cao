# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV TZ=Asia/Ho_Chi_Minh
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "do-an-server-nang-cao.dll"]
