using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capa_Entidades
{
    [Table("Inventario")]
    public class Inventario
    {
        [Key]
        public int Id_Inventario { get; set; }

        [Required(ErrorMessage = "La Descripcion es Obligatorio")]
        [StringLength(150, ErrorMessage = "La Descripcion no puede tener más de 150 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El Precio es Obligatorio")]
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        //poner el Id en el enpont opcional
        public int? Id_Producto { get; set; }

        //También opcional
        public string? Nombre_Producto { get; set; }
    }
}
