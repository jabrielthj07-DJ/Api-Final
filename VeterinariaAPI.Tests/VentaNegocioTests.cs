using Xunit;
using Capa_Negocio;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;

namespace VeterinariaAPI.Tests
{
    public class VentaNegocioTests
    {
        private readonly Venta_Negocio _ventaNegocio;

        public VentaNegocioTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:Connection", "Server=(local);Database=Test;Trusted_Connection=True;"}
                })
                .Build();

            _ventaNegocio = new Venta_Negocio(config);
        }

        [Fact]
        public void InsertarVenta_VentaNula_LanzaArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ventaNegocio.InsertarVenta(null);
            });
        }

        [Fact]
        public void InsertarVenta_CantidadMenorOIgualACero_LanzaArgumentException()
        {
            var venta = new Ventas
            {
                Id_Usuario = 1,
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta
                    {
                        Id_Producto = 1,
                        Cantidad = 0,
                        Precio_Unitario = 100
                    }
                }
            };

            Assert.Throws<ArgumentException>(() =>
            {
                _ventaNegocio.InsertarVenta(venta);
            });
        }
    }
}