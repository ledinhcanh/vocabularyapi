# Bước build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["lc_api.csproj", "./"]
RUN dotnet restore "lc_api.csproj"
COPY . .
RUN dotnet publish "lc_api.csproj" -c Release -o /app/publish

# Bước chạy thực tế
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "lc_api.dll"]
