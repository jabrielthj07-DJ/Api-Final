using Capa_Negocio;
using MongoDB.Driver;
using veterinariweb.Models.Metricas;

namespace veterinariweb.Service.MongoDB
{
    public class TicketPromedioService
    {
        private readonly MongoDBService _mongoDB;
        private readonly Venta_Negocio _ventaNegocio;

        private readonly IMongoCollection<TicketPromedioMetric> _coleccion;

        public TicketPromedioService(
            MongoDBService mongoDB,
            Venta_Negocio ventaNegocio)
        {
            _mongoDB = mongoDB;
            _ventaNegocio = ventaNegocio;

            _coleccion = _mongoDB.Database
                .GetCollection<TicketPromedioMetric>("ticketPromedioVentas");
        }

        // AGREGADO: calcular y almacenar ticket promedio diario
        public async Task<TicketPromedioMetric?> CalcularTicketPromedioAsync(
            DateTime fecha)
        {
            var ventas = _ventaNegocio.GetVentasConDetalles()
                .Where(v => v.Fecha_Venta.Date == fecha.Date)
                .ToList();

            if (!ventas.Any())
                return null;

            decimal totalVentas = ventas.Sum(v => v.Total);

            int cantidadVentas = ventas.Count;

            decimal ticketPromedio = Math.Round(
                totalVentas / cantidadVentas, 2);

            string id = fecha.ToString("yyyy-MM-dd");

            var metrica = new TicketPromedioMetric
            {
                Id = id,
                Fecha = fecha.Date,
                TotalVentas = totalVentas,
                CantidadVentas = cantidadVentas,
                TicketPromedio = ticketPromedio,
                Periodo = "Diario"
            };

            var filtro = Builders<TicketPromedioMetric>.Filter
                .Eq(x => x.Id, id);

            await _coleccion.ReplaceOneAsync(
                filtro,
                metrica,
                new ReplaceOptions { IsUpsert = true });

            return metrica;
        }

        // AGREGADO: consultar métrica almacenada
        public async Task<TicketPromedioMetric?> ObtenerTicketPromedioAsync(
            DateTime fecha)
        {
            string id = fecha.ToString("yyyy-MM-dd");

            return await _coleccion
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
