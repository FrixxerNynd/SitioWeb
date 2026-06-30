# Docker

## Resumen

Múltiples ajustes en imagen de SQLServer y optimización de Redis.

## 1. Ajustes en imagen de SQLServer y optimización de Redis

**Archivo:** `backend/docker-compose.yaml`

### Problema

* El servicio de Redis utilizaba la versión `redis:latest`, la cual no es óptima en tamaño y expone al entorno a cambios no deseados al actualizarse automáticamente de forma descontrolada.
* No se contaba con un contenedor de SQL Server local en el archivo Compose para el desarrollo, lo que requería configurar la base de datos de manera externa.

### Solución

1. Migrar la imagen de Redis a una versión específica y ligera basada en Alpine Linux (`redis:8.8.0-alpine3.23`).
2. Agregar un nuevo servicio de base de datos llamado `sqlserver` utilizando la imagen oficial de SQL Server 2025 (`mcr.microsoft.com/mssql/server:2025-latest`).
3. Configurar variables de entorno iniciales para la contraseña del `SA`, la aceptación de los términos de la licencia (`EULA`), y el volumen persistente `sqlserver-data` para evitar pérdidas de datos al recrear el contenedor.

### Código — Antes

```yaml
services:
  redis:
    image: redis:latest
    container_name: redis-server
    restart: always
    ports:
      - "6379:6379"
    command: redis-server --appendonly no
```

### Código — Después

```yaml
services:
  redis:
    image: redis:8.8.0-alpine3.23
    container_name: redis-server
    restart: always
    ports:
      - "6379:6379"
    command: redis-server --appendonly no
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2025-latest
    container_name: sqlserver
    restart: always
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=yourStrongPassword123!
      - ACCEPT_EULA=Y
      - MSSQL_PID=Developer
    volumes:
      - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:
```

**Motivo:** Mejorar la eficiencia y el control de versiones de las imágenes empleando Alpine para Redis, e integrar un entorno local autocontenido y persistente de base de datos con SQL Server 2025 para agilizar el desarrollo local.
