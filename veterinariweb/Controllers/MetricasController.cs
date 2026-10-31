using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using veterinariweb.Service.MongoDB;


namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class MetricasController : ControllerBase
    {
        private readonly TicketPromedioService _ticketService;

        public MetricasController(TicketPromedioService ticketService)
        {
            _ticketService = ticketService;
        }

        //  calcula y guarda ticket promedio
        [HttpPost("ticket-promedio")]
        public async Task<IActionResult> CalcularTicketPromedio(
            [FromQuery] DateTime? fecha)
        {
            try
            {
                DateTime fechaConsulta = fecha?.Date ?? DateTime.Today;

                var resultado =
                    await _ticketService.CalcularTicketPromedioAsync(
                        fechaConsulta);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No existen ventas registradas para esa fecha."
                    });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al calcular el ticket promedio.",
                    detalle = ex.Message
                });
            }
        }

        // consulta el ticket promedio almacenado
        [HttpGet("ticket-promedio")]
        public async Task<IActionResult> ObtenerTicketPromedio(
            [FromQuery] DateTime? fecha)
        {
            try
            {
                DateTime fechaConsulta = fecha?.Date ?? DateTime.Today;

                var resultado =
                    await _ticketService.ObtenerTicketPromedioAsync(
                        fechaConsulta);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No existe una métrica almacenada para esa fecha."
                    });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al consultar el ticket promedio.",
                    detalle = ex.Message
                });
            }
        }
    }
}
