# Validators - Validadores con FluentValidation

Validadores personalizados para DTOs usando FluentValidation. Concepto específico de .NET para validación avanzada.

## 📋 Responsabilidades

- Validar datos de entrada (DTOs)
- Implementar reglas de validación complejas
- Proporcionar mensajes de error personalizados
- Validación asíncrona cuando sea necesario

## 🔗 Conexiones

- **Valida**: DTOs (objetos de entrada)
- **Usado por**: Controllers (automáticamente)
- **Configurado en**: Program.cs (DI container)
- **Puede acceder**: Base de datos para validaciones asíncronas

## 🛠️ Tipos de Validadores

### Por DTOs de Entrada
- `EmpleadoCreateDtoValidator.cs`
- `ClienteUpdateDtoValidator.cs`
- `TicketCreateDtoValidator.cs`

### Validaciones Comunes
- `EmailValidator.cs` - Validación de emails
- `PhoneValidator.cs` - Validación de teléfonos
- `PasswordValidator.cs` - Validación de passwords

## 💡 Ejemplo de Estructura

```csharp
public class EmpleadoCreateDtoValidator : AbstractValidator<EmpleadoCreateDto>
{
    public EmpleadoCreateDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");
    }
}
```

## 🔧 Reglas Disponibles

### Validaciones Básicas
- `NotEmpty()` - No vacío
- `MaximumLength()` - Longitud máxima
- `MinimumLength()` - Longitud mínima
- `EmailAddress()` - Formato de email

### Validaciones Personalizadas
- `Must()` - Validación personalizada
- `MustAsync()` - Validación asíncrona
- `Custom()` - Validación completamente personalizada

### Validaciones de Rango
- `GreaterThan()` - Mayor que
- `LessThan()` - Menor que
- `InclusiveBetween()` - Entre valores

## ⚙️ Características

- Integración automática con ASP.NET Core
- Mensajes de error en español
- Validación asíncrona para base de datos
- Reglas condicionales
- Validación en cascada