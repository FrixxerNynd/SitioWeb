public class RecuperacionPasswordToken
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public DateTime Expiracion { get; set; }
    public bool Usado { get; set; }
}

