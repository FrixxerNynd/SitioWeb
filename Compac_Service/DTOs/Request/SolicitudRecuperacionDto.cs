/// <summary>
/// DTO para solicitar recuperación de contraseña.
/// Se puede identificar la cuenta por email o por RFC (clientes legacy).
/// </summary>
public class SolicitudRecuperacionDto
{
    /// <summary>Correo electrónico registrado (usuarios internos o clientes legacy)</summary>
    public string? Email { get; set; }

    /// <summary>RFC del cliente legacy (alternativa al email)</summary>
    public string? Rfc { get; set; }
}

/// <summary>
/// DTO para cambiar la contraseña usando un token de recuperación de un solo uso.
/// </summary>
public class CambioContrasenaRecuperacionDto
{
    public required string Email { get; set; }
    public required string NuevoPassword { get; set; }
    public required string Token { get; set; }
}