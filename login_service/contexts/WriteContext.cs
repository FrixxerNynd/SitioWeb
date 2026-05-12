using back_cabs.CRM.models.Auth;

using back_cabs.CRM.models;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace back_cabs.CRM.contexts;

/// <summary>
/// Contexto básico para operaciones de escritura (POST, PUT, DELETE)
/// </summary>
public class WriteContext : DbContext

{
    public WriteContext(DbContextOptions<WriteContext> options) : base(options)
    {
        // Configuraciones específicas para escritura
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        ChangeTracker.AutoDetectChangesEnabled = true;
    }

    /// <summary>
    /// Usuarios del sistema de autenticación
    /// </summary>
    public DbSet<UsuarioAuth> UsuariosAuth { get; set; } = null!;

    /// <summary>
    /// Tokens de recuperación de contraseña
    /// </summary>
    public DbSet<RecuperacionPasswordToken> RecuperacionPasswordTokens { get; set; } = null!;


    // ═══════════════════════════════════════════════════════════════

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración simplificada de UsuarioAuth - usa los atributos del modelo
        modelBuilder.Entity<UsuarioAuth>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

    
    }
}