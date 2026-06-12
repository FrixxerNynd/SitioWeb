/// <summary>
/// Token de recuperación de contraseña de un solo uso.
/// NOTA: Al agregar TipoUsuario se requiere migración de BD:
///   dotnet ef migrations add AddTipoUsuarioToRecuperacionToken
///   dotnet ef database update
/// </summary>
public class RecuperacionPasswordToken
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public DateTime Expiracion { get; set; }
    public bool Usado { get; set; }

    /// <summary>
    /// Origen de la cuenta: "INTERNO" (UsuariosAuth) o "CLIENTE_LEGACY" (AdmClientes)
    /// </summary>
    public string TipoUsuario { get; set; } = "INTERNO";
}
