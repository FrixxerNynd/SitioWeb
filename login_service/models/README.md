# Models - Entidades de Dominio

Los modelos representan las entidades de la base de datos. Similar a los models en Mongoose o Sequelize.

## 📋 Responsabilidades

- Definir estructura de datos
- Establecer relaciones entre entidades
- Contener propiedades y validaciones básicas
- Representar tablas de base de datos

## 🔗 Conexiones

- **Usados por**: Services, Contexts (Entity Framework)
- **Se mapean a**: DTOs (para transferencia)
- **Relacionados con**: Otros Models (navegación)
- **Configurados en**: Contexts (OnModelCreating)

## 📁 Organización por Módulos

### Administracion/
- `Empleado.cs` - Entidad empleado
- `Rol.cs` - Entidad rol
- `Departamento.cs` - Entidad departamento

### Recepcion/
- `Cliente.cs` - Entidad cliente
- `Pedido.cs` - Entidad pedido
- `DetallePedido.cs` - Detalles de pedido

### Soporte/
- `Ticket.cs` - Entidad ticket
- `MensajeTicket.cs` - Entidad mensaje
- `CategoriaTicket.cs` - Entidad categoría

## 💡 Ejemplo de Estructura

```csharp
public class Empleado : BaseEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Relaciones
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
}
```

## ⚙️ Características

- Herencia de `BaseEntity` (auditoría automática)
- Propiedades de navegación para relaciones
- Validaciones básicas con Data Annotations
- Configuración en Entity Framework