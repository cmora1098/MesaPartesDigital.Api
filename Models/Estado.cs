using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesaPartesDigital.Models
{
    [Table("T_Estado")] // 👈 Nombre de tu tabla en SQL Server
    public class Estado
    {
        [Key]
        [Column("iCodEstado")] // 👈 Tu Llave Primaria
        public int ICodEstado { get; set; }

        [Column("vNombreEstado")]
        public string VNombreEstado { get; set; } = string.Empty;

        [Column("bActivo")]
        public bool BActivo { get; set; }
    }
}