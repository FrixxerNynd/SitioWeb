# Services - Lógica de Negocio

Los servicios contienen toda la lógica de negocio del sistema. Similar a los services en Node.js/NestJS.

## 📋 Responsabilidades

- Implementar reglas de negocio
- Coordinar operaciones complejas
- Llamar a repositorios/contextos de datos
- Manejar transacciones
- Aplicar validaciones de negocio

## 🔗 Conexiones

- **Recibe llamadas de**: Controllers
- **Llama a**: Contexts (base de datos), otros Services
- **Usa**: Models (entidades), DTOs
- **Implementa**: Interfaces (IEmpleadoService, etc.)

## 📁 Organización por Módulos

### Administracion/
- `EmpleadoService.cs` - Lógica de empleados
- `RolService.cs` - Lógica de roles y permisos
- `DepartamentoService.cs` - Lógica de departamentos

### Recepcion/
- `ClienteService.cs` - Lógica de clientes
- `PedidoService.cs` - Lógica de pedidos y facturación

### Soporte/
- `TicketService.cs` - Lógica de tickets y asignaciones
- `MensajeService.cs` - Lógica de mensajes y notificaciones

## 💡 Ejemplo de Estructura

```csharp
public class EmpleadoService : IEmpleadoService
{
    private readonly WriteContext _writeContext;
    private readonly ReadOnlyContext _readContext;

    public async Task<EmpleadoResponseDto> CreateEmpleado(EmpleadoCreateDto dto)
    {
        // 1. Validaciones de negocio
        // 2. Crear entidad
        // 3. Guardar en BD
        // 4. Devolver DTO
    }
}
```

## ⚙️ Configuración

- Inyección de dependencias
- Uso de contextos separados (Read/Write)
- Manejo de excepciones personalizadas
- Logging con Serilog