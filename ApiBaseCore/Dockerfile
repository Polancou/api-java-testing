# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de solución y los proyectos
COPY ["ApiBaseCore.sln", "./"]
COPY ["ApiBaseCore.Api/ApiBaseCore.Api.csproj", "ApiBaseCore.Api/"]
COPY ["ApiBaseCore.Application/ApiBaseCore.Application.csproj", "ApiBaseCore.Application/"]
COPY ["ApiBaseCore.Domain/ApiBaseCore.Domain.csproj", "ApiBaseCore.Domain/"]
COPY ["ApiBaseCore.Infrastructure/ApiBaseCore.Infrastructure.csproj", "ApiBaseCore.Infrastructure/"]
COPY ["ApiBaseCore.Api.IntegrationTests/ApiBaseCore.Api.IntegrationTests.csproj", "ApiBaseCore.Api.IntegrationTests/"]
COPY ["ApiBaseCore.Application.UnitTests/ApiBaseCore.Application.UnitTests.csproj", "ApiBaseCore.Application.UnitTests/"]

# Restaurar dependencias
RUN dotnet restore "ApiBaseCore.sln"

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar la API (Release)
WORKDIR "/src/ApiBaseCore.Api"
RUN dotnet publish "ApiBaseCore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (Imagen final más ligera)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Crear carpeta para uploads (según tu FileStorageService)
# Esto asegura que los permisos sean correctos dentro del contenedor
RUN mkdir -p /app/wwwroot/uploads/avatars

COPY --from=build /app/publish .

# Exponer puerto 8080 (puerto por defecto en .NET 8/9 containers)
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiBaseCore.Api.dll"]