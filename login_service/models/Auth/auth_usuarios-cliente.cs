using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Auth
{
    [Table("auth_usuarios-clientes")]
    public class Auth_cliente
    {
        [Key]
        [Column("Id_Cliente")]
        public int Id_Cliente { get; set; }

        [Required]
        [Column("password")]
        public string password { get; set;} = string.Empty;
    }
}