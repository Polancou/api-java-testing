# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de solución y los proyectos
COPY ["EcommerceCore.sln", "./"]
COPY ["EcommerceCore.Api/EcommerceCore.Api.csproj", "EcommerceCore.Api/"]
COPY ["EcommerceCore.Application/EcommerceCore.Application.csproj", "EcommerceCore.Application/"]
COPY ["EcommerceCore.Domain/EcommerceCore.Domain.csproj", "EcommerceCore.Domain/"]
COPY ["EcommerceCore.Infrastructure/EcommerceCore.Infrastructure.csproj", "EcommerceCore.Infrastructure/"]
COPY ["EcommerceCore.Api.IntegrationTests/EcommerceCore.Api.IntegrationTests.csproj", "EcommerceCore.Api.IntegrationTests/"]
COPY ["EcommerceCore.Application.UnitTests/EcommerceCore.Application.UnitTests.csproj", "EcommerceCore.Application.UnitTests/"]

# Restaurar dependencias
RUN dotnet restore "EcommerceCore.sln"

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar la API (Release)
WORKDIR "/src/EcommerceCore.Api"
RUN dotnet publish "EcommerceCore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (Imagen final más ligera)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Crear carpeta para uploads (según tu FileStorageService)
# Esto asegura que los permisos sean correctos dentro del contenedor
RUN mkdir -p /app/wwwroot/uploads/avatars

COPY --from=build /app/publish .

# Exponer puerto 8080 (puerto por defecto en .NET 8/9 containers)
EXPOSE 8080

ENTRYPOINT ["dotnet", "EcommerceCore.Api.dll"]