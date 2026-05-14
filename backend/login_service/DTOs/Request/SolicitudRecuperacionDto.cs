using Org.BouncyCastle.Bcpg.OpenPgp;

public class SolicitudRecuperacionDto
{
    public required string Email { get; set; }
}

public class CambioContrasenaRecuperacionDto
{
    public required string Email { get; set; }
    public required string NuevoPassword { get; set; }
    public required string Token { get; set; }
}