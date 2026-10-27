using System;
using System.ComponentModel.DataAnnotations;

namespace Capa_Entidades
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre correo es obligatorio.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Clave { get; set; } = string.Empty;
    }
}
