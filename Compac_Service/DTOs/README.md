# DTOs - Data Transfer Objects

Los DTOs definen la estructura de datos que viajan entre cliente y servidor. Concepto específico de .NET.

## 📋 Responsabilidades

- Definir contratos de API
- Encapsular datos de entrada y salida
- Evitar exposer entidades internas
- Facilitar versionado de APIs

## 🔗 Conexiones

- **Usados por**: Controllers (request/response)
- **Mapeados desde/hacia**: Models (entidades)
- **Validados por**: Validators (FluentValidation)
- **Documentados en**: Swagger automáticamente

## 📁 Organización por Módulos

### Administracion/
- `EmpleadoCreateDto.cs` - Para crear empleado
- `EmpleadoUpdateDto.cs` - Para actualizar empleado
- `EmpleadoResponseDto.cs` - Para respuestas

### Recepcion/
- `ClienteCreateDto.cs` - Para crear cliente
- `PedidoCreateDto.cs` - Para crear pedido
- `PedidoResponseDto.cs` - Para respuestas de pedido

### Soporte/
- `TicketCreateDto.cs` - Para crear ticket
- `MensajeCreateDto.cs` - Para crear mensaje
- `TicketResponseDto.cs` - Para respuestas de ticket

## 💡 Tipos de DTOs

### Request DTOs (Entrada)
- `{Entidad}CreateDto` - Para operaciones POST
- `{Entidad}UpdateDto` - Para operaciones PUT
- `{Entidad}FilterDto` - Para filtros de búsqueda

### Response DTOs (Salida)
- `{Entidad}ResponseDto` - Para respuestas individuales
- `{Entidad}ListDto` - Para listas
- `{Entidad}SummaryDto` - Para resúmenes

## 💡 Ejemplo de Estructura

```csharp
public class EmpleadoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
}

public class EmpleadoResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
```

## ⚙️ Buenas Prácticas

- Nunca exponer IDs internos innecesarios
- Usar nombres descriptivos
- Separar DTOs de entrada y salida
- Incluir solo campos necesarios