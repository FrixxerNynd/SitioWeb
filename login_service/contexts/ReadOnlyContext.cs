using back_cabs.CRM.models.Auth;

using back_cabs.CRM.models;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.contexts;

/// <summary>
/// Contexto básico para operaciones de solo lectura (GET queries)
/// </summary>
public class ReadOnlyContext : DbContext
{
    public ReadOnlyContext(DbContextOptions<ReadOnlyContext> options) : base(options)
    {
        // Configuraciones específicas para solo lectura
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    /// <summary>
    /// Usuarios del sistema de autenticación
    /// </summary>
    public DbSet<UsuarioAuth> UsuariosAuth { get; set; } = null!;

    /// <summary>
    /// Tokens de recuperación de contraseña
    /// </summary>
    public DbSet<RecuperacionPasswordToken> RecuperacionPasswordTokens { get; set; } = null!;

  
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