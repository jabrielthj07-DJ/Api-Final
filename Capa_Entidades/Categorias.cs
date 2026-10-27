using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capa_Entidades
{
    [Table("Categorias")]
    public class Categorias
    {
        [Key]

        public int Id_Categoria { get; set; }
        public string Nombre_Categoria { get; set; }
        public string Descripcion { get; set; }
    }
}
