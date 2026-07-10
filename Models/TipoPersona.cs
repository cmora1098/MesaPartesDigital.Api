using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesaPartesDigital.Models
{
    [Table("T_TipoPersona")]
    public class TipoPersona
    {
        [Key]
        [Column("iCodTipoPer")]
        public int ICodTipoPer { get; set; }

        [Column("vDescTipoPer")]
        public string VDescTipoPer { get; set; } = string.Empty;

        [Column("bActivo")]
        public bool BActivo { get; set; }
    }
}