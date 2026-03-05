# Bước build (Nâng cấp lên SDK 9.0 mới nhất)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Copy toàn bộ code vào
COPY . .
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# Bước chạy thực tế (Nâng cấp Runtime lên 9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
