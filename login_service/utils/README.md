# Utils - Utilidades y Helpers

Funciones y clases de utilidad reutilizables en todo el proyecto. Similar a la carpeta utils en proyectos JavaScript.

## 📋 Responsabilidades

- Funciones auxiliares reutilizables
- Extensiones de tipos existentes
- Helpers para operaciones comunes
- Constantes y configuraciones

## 🔗 Conexiones

- **Usado por**: Services, Controllers, Middleware
- **Proporciona**: Funciones estáticas, extensiones
- **No depende de**: Entidades específicas de negocio
- **Facilita**: Código limpio y reutilizable

## 🛠️ Tipos de Utilidades

### Extensiones
- `StringExtensions.cs` - Extensiones para strings
- `DateTimeExtensions.cs` - Extensiones para fechas
- `EnumExtensions.cs` - Extensiones para enums

### Helpers
- `PasswordHelper.cs` - Hash y validación de passwords
- `EmailHelper.cs` - Validación y formato de emails
- `FileHelper.cs` - Manejo de archivos
- `JsonHelper.cs` - Serialización/deserialización

### Mappers
- `AutoMapperProfile.cs` - Configuración de AutoMapper
- `DtoMapper.cs` - Mapeo manual entre DTOs y entidades

### Constantes
- `AppConstants.cs` - Constantes de la aplicación
- `ErrorMessages.cs` - Mensajes de error
- `ApiRoutes.cs` - Rutas de API como constantes

## 💡 Ejemplo de Estructura

```csharp
public static class StringExtensions
{
    public static bool IsValidEmail(this string email)
    {
        // Validación de email
    }

    public static string ToSafeFileName(this string input)
    {
        // Convertir a nombre de archivo seguro
    }
}

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        // Hash de password con BCrypt
    }
}
```

## 📁 Organización

- `Extensions/` - Métodos de extensión
- `Helpers/` - Clases helper estáticas
- `Constants/` - Constantes de la aplicación
- `Mappers/` - Mapeo de objetos

## ⚙️ Buenas Prácticas

- Funciones estáticas cuando sea posible
- Sin dependencias de negocio
- Bien documentado y testeado
- Reutilizable entre módulos