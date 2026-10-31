
using MongoDB.Bson.Serialization.Attributes;

namespace veterinariweb.Models.Metricas
{
    public class TicketPromedioMetric
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal TotalVentas { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TicketPromedio { get; set; }

        public string Periodo { get; set; } = "Diario";
    }
}
