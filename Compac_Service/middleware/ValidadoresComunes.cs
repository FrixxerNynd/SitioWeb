
// =====================================================================================
// VALIDADORES COMUNES - ValidadoresComunes.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define validadores reutilizables para datos comunes como GUIDs, emails, contraseñas,
// teléfonos y rangos de fechas usando FluentValidation. Incluye extensiones para reglas
// personalizadas y validaciones consistentes en toda la aplicación.
//
// ¿CÓMO SE USA?
// - Se utiliza GuidValidator, EmailValidator, PasswordValidator, etc. en los modelos de entrada
// - Se aplican extensiones como IsValidGuid(), IsStrongPassword() en reglas personalizadas
//
// ¿EN QUÉ CASOS SE USA?
// - Validación de IDs, emails, contraseñas y teléfonos en endpoints de API
// - Validación de rangos de fechas en filtros y reportes
// - Reglas DRY y consistentes en múltiples controladores y servicios
//
// VENTAJAS:
// - Centraliza reglas de validación comunes
// - Facilita mantenimiento y cambios de reglas
// - Reduce duplicidad y errores en validaciones
// =====================================================================================
using FluentValidation;

namespace back_cabs.CRM.Middleware;


/// <summary>
/// Validador para cadenas que deben ser GUIDs válidos
/// </summary>
public class GuidValidator : AbstractValidator<string>
{
    public GuidValidator()
    {
        // Regla: No vacío y debe ser un GUID válido
        RuleFor(x => x)
            .NotEmpty().WithMessage("El ID no puede estar vacío")
            .Must(BeValidGuid).WithMessage("El ID debe ser un GUID válido");
    }

    // Valida si la cadena es un GUID válido
    private bool BeValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}


/// <summary>
/// Validador para emails con formato y longitud estándar
/// </summary>
public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        // Regla: No vacío, formato email válido, longitud máxima 254
        RuleFor(x => x)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(254).WithMessage("El email no puede tener más de 254 caracteres");
    }
}


/// <summary>
/// Validador para contraseñas fuertes con requisitos configurables
/// </summary>
public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator(int minLength = 8)
    {
        // Regla: No vacío, mínimo de longitud, mayúscula, minúscula, número y especial
        RuleFor(x => x)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(minLength).WithMessage($"La contraseña debe tener al menos {minLength} caracteres")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
            .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial");
    }
}


/// <summary>
/// Validador para números de teléfono en formato internacional E.164
/// </summary>
public class PhoneNumberValidator : AbstractValidator<string>
{
    public PhoneNumberValidator()
    {
        // Regla: No vacío, formato internacional, máximo 15 dígitos
        RuleFor(x => x)
            .NotEmpty().WithMessage("El número de teléfono es requerido")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del número de teléfono no es válido")
            .MaximumLength(15).WithMessage("El número de teléfono no puede tener más de 15 dígitos");
    }
}


/// <summary>
/// Validador para rangos de fechas (inicio y fin)
/// </summary>
public class DateRangeValidator : AbstractValidator<(DateTime StartDate, DateTime EndDate)>
{
    public DateRangeValidator()
    {
        // Regla: Fecha de inicio <= fecha de fin y viceversa
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior o igual a la fecha de inicio");
    }
}


/// <summary>
/// Extensiones para reglas de validación comunes reutilizables
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Regla para validar que una cadena sea un GUID válido
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidGuid<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(x => Guid.TryParse(x, out _)).WithMessage("Debe ser un GUID válido");
    }

    /// <summary>
    /// Regla para validar contraseñas fuertes con requisitos configurables
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsStrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder, int minLength = 8)
    {
        return ruleBuilder
            .MinimumLength(minLength).WithMessage($"Debe tener al menos {minLength} caracteres")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una letra mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe contener al menos una letra minúscula")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número")
            .Matches(@"[\W_]").WithMessage("Debe contener al menos un carácter especial");
    }

    /// <summary>
    /// Regla para validar emails con formato estándar
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El formato del email no es válido");
    }

    /// <summary>
    /// Regla para validar números de teléfono en formato internacional
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del número de teléfono no es válido");
    }
}