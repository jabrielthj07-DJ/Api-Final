using Capa_Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace veterinariweb.Controllers
{
    /// <summary>
    /// Permite a un usuario Iniciar Sesion y generar Token JWt
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly MiDbContext _context;
        private readonly IConfiguration _config;

        public LoginController(MiDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Iniciar Sesion
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // 1. Buscar usuario en la BD
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo_Electronico == request.Correo);

                if (usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no encontrado" });

                // Validar que la contraseña en la BD no sea nula
                if (string.IsNullOrEmpty(usuario.Contraseña))
                    return StatusCode(500, new { mensaje = "El usuario no tiene una contraseña configurada en la BD." });

                // 2. Validar clave con BCrypt (protegido contra texto plano o formato inválido)
                bool ok = false;
                try
                {
                    ok = BCrypt.Net.BCrypt.Verify(request.Clave, usuario.Contraseña);
                }
                catch (Exception)
                {
                    // Si la contraseña en la BD no era un hash BCrypt válido
                    return StatusCode(500, new
                    {
                        mensaje = "La contraseña en la BD no tiene formato BCrypt válido. Asegúrate de haberla registrado encriptada."
                    });
                }

                if (!ok)
                    return Unauthorized(new { mensaje = "Credenciales inválidas" });

                // 3. Generar Token (con clave de respaldo si falta en configuración)
                var secretKey = _config["Jwt:Key"] ?? "MiClaveMuySegura1234567890123456";
                var issuer = _config["Jwt:Issuer"] ?? "miApp";
                var audience = _config["Jwt:Audience"] ?? "miAppUsuarios";

                var key = Encoding.UTF8.GetBytes(secretKey);

                // Obtener el nombre del rol desde la tabla Roles
                var rol = _context.Roles
                    .Where(r => r.Id_Rol == usuario.Id_Rol)
                    .Select(r => r.cargo)
                    .FirstOrDefault() ?? "SinRol";

                // Claims protegidos contra nulls
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo_Electronico ?? ""),
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? "Usuario"),
                    new Claim(ClaimTypes.Role, rol),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    Token = tokenString,
                    Usuario = usuario.Nombre,
                    Correo = usuario.Correo_Electronico,
                    Rol_Id = usuario.Id_Rol,
                    Rol = rol
                });
            }
            catch (Exception ex)
            {
                // Si la BD no conecta (timeout, firewall, etc.), devolverá el detalle exacto
                return StatusCode(500, new
                {
                    mensaje = "Error interno en el servidor",
                    detalle = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Login/datos
        [Authorize]
        [HttpGet("datos")]
        public IActionResult GetDatos()
        {
            return Ok("Acceso autorizado");
        }
    }
}