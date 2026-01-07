# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Copy toàn bộ source code vào container
COPY . .

# 2. Restore dependencies (Dựa trên DoAn.sln đã được copy vào)
RUN dotnet restore

# 3. Publish project API (Release mode)
# Lưu ý: Đường dẫn trỏ đúng vào file csproj của API
RUN dotnet publish src/DoAn.Presentation.Api/DoAn.Presentation.Api.csproj -c Release -o /app/out

# Stage 2: Runtime (Chạy ứng dụng)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port (Optional, thông thường là 8080 với .NET 8)
EXPOSE 8080

# 4. Chạy file DLL (Tên file dựa trên tên project API)
ENTRYPOINT ["dotnet", "DoAn.Presentation.Api.dll"]