# Bước build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sửa thành API.csproj
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Copy toàn bộ code vào
COPY . .
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# Bước chạy thực tế
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Sửa thành API.dll
ENTRYPOINT ["dotnet", "API.dll"]
