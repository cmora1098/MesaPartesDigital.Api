using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesaPartesDigital.Models
{
    [Table("T_TipoDocPer")]
    public class TipoDocPer
    {
        [Key]
        [Column("iCodTipoDocPer")]
        public int ICodTipoDocPer { get; set; }

        [Column("vDescTipoDoc")]
        public string VDescTipoDoc { get; set; } = string.Empty;

        [Column("bActivo")]
        public bool BActivo { get; set; }
    }
}