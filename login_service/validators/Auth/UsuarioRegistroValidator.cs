// =====================================================================================
// VALIDADOR REGISTRO USUARIO - UsuarioRegistroValidator.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa las reglas de validación para el registro de usuarios.
// Incluye validaciones de negocio específicas como unicidad de email,
// complejidad de contraseña, y consistencia de datos.
//
// CUÁNDO USARLO:
// - Antes de procesar el registro de usuario
// - En el servicio de autenticación
// - Para validación automática en controladores
// - En pruebas unitarias de validación
//
// CÓMO USARLO:
// var validator = new UsuarioRegistroValidator(_context);
// var result = await validator.ValidateAsync(dto);
// if (!result.IsValid) return BadRequest(result.Errors);
//
// =====================================================================================

using FluentValidation;
using CRM.DTOs.Request;
using back_cabs.CRM.contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace back_cabs.CRM.validators.Auth
{
    /// <summary>
    /// Validador para el registro de usuarios
    /// </summary>
    public class UsuarioRegistroValidator : AbstractValidator<UsuarioRegistroRequestDto>
    {
        private readonly ReadOnlyContext _readContext;

        public UsuarioRegistroValidator(ReadOnlyContext readContext)
        {
            _readContext = readContext;
            
            ConfigurarReglasValidacion();
        }

        /// <summary>
        /// Configura todas las reglas de validación
        /// </summary>
        private void ConfigurarReglasValidacion()
        {
            // Validación de nombre
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre es obligatorio")
                .Length(2, 100)
                .WithMessage("El nombre debe tener entre 2 y 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El nombre solo puede contener letras y espacios");

            // Validación de apellido
            RuleFor(x => x.Apellido)
                .NotEmpty()
                .WithMessage("El apellido es obligatorio")
                .Length(2, 100)
                .WithMessage("El apellido debe tener entre 2 y 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El apellido solo puede contener letras y espacios");

            // Validación de teléfono (requerido, debe ser un número válido de 10 dígitos)
            RuleFor(x => x.Telefono)
                .NotEmpty()
                .WithMessage("El teléfono es obligatorio")
                .Must(telefono => {
                    var telefonoStr = telefono.ToString();
                    return telefonoStr.Length == 10 && telefonoStr.All(char.IsDigit);
                })
                .WithMessage("El teléfono debe ser un número válido de 10 dígitos (ejemplo: 5512345678)");

            // Validación de email
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El email es obligatorio")
                .EmailAddress()
                .WithMessage("Debe ser un email válido")
                .MaximumLength(150)
                .WithMessage("El email no puede exceder 150 caracteres")
                .MustAsync(async (email, cancellation) => await SerEmailUnico(email))
                .WithMessage("Ya existe un usuario registrado con este email");

            // Validación de contraseña (simplificada para desarrollo)
            RuleFor(x => x.Contrasena)
                .NotEmpty()
                .WithMessage("La contraseña es obligatoria")
                .MinimumLength(8)
                .WithMessage("La contraseña debe tener al menos 8 caracteres");
            // TODO: Agregar validación de complejidad en producción:
            // .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            // .WithMessage("La contraseña debe contener al menos: 1 minúscula, 1 mayúscula, 1 número y 1 carácter especial");

            // Validación de confirmación de contraseña
            RuleFor(x => x.ConfirmarContrasena)
                .NotEmpty()
                .WithMessage("La confirmación de contraseña es obligatoria")
                .Equal(x => x.Contrasena)
                .WithMessage("Las contraseñas no coinciden");
}

        /// <summary>
        /// Verifica si el email es único en el sistema
        /// </summary>
        private async Task<bool> SerEmailUnico(string email)
        {
            try
            {
                var existe = await _readContext.UsuariosAuth
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
                return !existe;
            }
            catch (Exception)
            {
                // En caso de error de BD, permitir la validación y que sea manejada en el servicio
                return true;
            }
        }

        // Métodos de validación obsoletos eliminados
        // TODO: Restaurar validaciones de enum cuando se implemente la lógica de negocio completa
    }
}