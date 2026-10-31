using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using veterinariweb.Service.MongoDB;

namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class MongoDBController : ControllerBase
    {
        private readonly MongoDBService _mongoDB;

        public MongoDBController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [HttpGet("estado")]
        public async Task<IActionResult> ComprobarConexion()
        {
            bool conectado = await _mongoDB.ProbarConexionAsync();

            if (!conectado)
                return StatusCode(503, new
                {
                    mensaje = "No se pudo conectar con MongoDB Atlas."
                });

            return Ok(new
            {
                mensaje = "Conexión exitosa con MongoDB Atlas.",
                baseDatos = "VeterinariaMLMetrics"
            });
        }
    }
}
