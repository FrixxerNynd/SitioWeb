// =====================================================================================
// ENTIDAD USUARIO AUTH - UsuarioAuth.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para usuarios del sistema CRM.
// Representa la tabla Auth_usuarios en la base de datos con todas sus propiedades
// y reglas de negocio encapsuladas.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Aplicación de reglas de negocio del dominio
// - Validaciones a nivel de entidad
//
// CÓMO USARLO:
// var usuario = new UsuarioAuth 
// { 
//     NombreCompleto = "Juan Pérez",
//     Email = "juan@empresa.com",
//     Rol = RolUsuario.Soporte.ToString()
// };
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Auth
{
    /// <summary>
    /// Entidad que representa un usuario del sistema CRM
    /// </summary>
    [Table("auth_usuarios")]
    public class UsuarioAuth
    {
        /// <summary>
        /// Identificador único del usuario (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro
        /// </summary>
        [Required]
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de última actualización del registro
        /// </summary>
        [Column("actualizado_en")]
        public DateTime? ActualizadoEn { get; set; }

        /// <summary>
        /// Email único del usuario (usado para login)
        /// </summary>
        [Required]
        [StringLength(150)]
        [Column("correo")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash de la contraseña del usuario
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        [Required]
        [Column("telefono")]
        public long Telefono { get; set; }

        /// <summary>
        /// Rol del usuario en el sistema (valor string)
        /// </summary>
        [Required]
        [StringLength(30)]
        [Column("rol")]
        public string Rol { get; set; } = "SUBADMIN"; // Valor por defecto

        /// <summary>
        /// Indica si el usuario está activo en el sistema
        /// </summary>
        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Tipo de transmisión que el usuario puede manejar
        /// </summary>
        [StringLength(50)]
        [Column("transmision_habilitada")]
        public string? TransmisionHabilitada { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // ENLACE CON SISTEMA LEGACY (adCABS2016.admAgentes)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del agente en el sistema legacy (adCABS2016.admAgentes.CIDAGENTE)
        /// NULL = Usuario sin agente legacy asociado
        /// </summary>
        [Column("id_agente_legacy")]
        public int? IdAgenteLegacy { get; set; }

        /// <summary>
        /// Código del agente legacy para referencia rápida (CCODIGOAGENTE)
        /// Se almacena para evitar consultas innecesarias a BD legacy
        /// </summary>
        [StringLength(30)]
        [Column("codigo_agente_legacy")]
        public string? CodigoAgenteLegacy { get; set; }

        /// <summary>
        /// Nombre del agente legacy (CNOMBREAGENTE)
        /// Cache local para mostrar sin consultar BD legacy
        /// </summary>
        [StringLength(60)]
        [Column("nombre_agente_legacy")]
        public string? NombreAgenteLegacy { get; set; }

        /// <summary>
        /// Fecha en que se realizó el enlace con el agente legacy
        /// </summary>
        [Column("fecha_enlace_agente")]
        public DateTime? FechaEnlaceAgente { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES CALCULADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Nombre completo del usuario (propiedad calculada)
        /// </summary>
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        /// <summary>
        /// Valida si el usuario puede usar vehículos de empresa
        /// </summary>
        [NotMapped]
        public bool PuedeUsarVehiculo => !string.IsNullOrWhiteSpace(TransmisionHabilitada) && TransmisionHabilitada != "Ninguna";

        /// <summary>
        /// Indica si el usuario tiene un agente legacy asociado
        /// </summary>
        [NotMapped]
        public bool TieneAgenteLegacy => IdAgenteLegacy.HasValue && IdAgenteLegacy.Value > 0;

        /// <summary>
        /// Actualiza la fecha de modificación
        /// </summary>
        public void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa el usuario
        /// </summary>
        public void Activar()
        {
            Activo = true;
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Desactiva el usuario
        /// </summary>
        public void Desactivar()
        {
            Activo = false;
            ActualizarFechaModificacion();
        }
    }
}