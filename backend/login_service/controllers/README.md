# Controllers - Controladores de API

Los controladores manejan las peticiones HTTP y devuelven respuestas. Similar a los controladores en Express.js.

## 📋 Responsabilidades

- Recibir peticiones HTTP (GET, POST, PUT, DELETE)
- Validar datos de entrada básicos
- Llamar a los servicios correspondientes
- Devolver respuestas HTTP apropiadas
- Manejar errores y códigos de estado

## 🔗 Conexiones

- **Recibe de**: Cliente HTTP (Frontend, Postman, etc.)
- **Llama a**: Services (lógica de negocio)
- **Usa**: DTOs para transferir datos
- **Responde con**: DTOs o códigos de estado HTTP

## 📁 Organización por Módulos

### Administracion/
- `EmpleadoController.cs` - CRUD de empleados
- `RolController.cs` - Gestión de roles
- `DepartamentoController.cs` - Gestión de departamentos

### Recepcion/
- `ClienteController.cs` - CRUD de clientes
- `PedidoController.cs` - Gestión de pedidos

### Soporte/
- `TicketController.cs` - Sistema de tickets
- `MensajeController.cs` - Mensajes de tickets

## 💡 Ejemplo de Estructura

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmpleadoController : ControllerBase
{
    private readonly IEmpleadoService _empleadoService;

    [HttpGet]
    public async Task<ActionResult<List<EmpleadoResponseDto>>> GetEmpleados()
    {
        // Llama al servicio y devuelve DTOs
    }
}
```

## ⚙️ Configuración

- Todos los controladores usan `[ApiController]`
- Rutas base: `api/[controller]`
- Inyección de dependencias para servicios
- Documentación automática con Swagger