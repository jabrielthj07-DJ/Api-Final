using Xunit;
using Capa_Negocio;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;

namespace VeterinariaAPI.Tests
{
    public class CompraNegocioTests
    {
        private readonly Compra_Negocio _compraNegocio;

        public CompraNegocioTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:Connection", "Server=(local);Database=Test;Trusted_Connection=True;"}
                })
                .Build();

            _compraNegocio = new Compra_Negocio(config);
        }

        [Fact]
        public void InsertarCompra_SinDetalles_LanzaArgumentNullException()
        {
            // Arrange
            var compra = new Compras
            {
                Id_Proveedor = 1,
                Detalles = new List<DetalleCompra>()
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _compraNegocio.InsertarCompra(compra));
        }

        [Fact]
        public void InsertarCompra_CantidadMenorOIgualACero_LanzaArgumentException()
        {
            // Arrange
            var compra = new Compras
            {
                Id_Proveedor = 1,
                Detalles = new List<DetalleCompra>
                {
                    new DetalleCompra
                    {
                        Id_Producto = 1,
                        Cantidad = 0,
                        Precio_Unitario = 50
                    }
                }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _compraNegocio.InsertarCompra(compra));
        }

        [Fact]
        public void InsertarCompra_PrecioMenorOIgualACero_LanzaArgumentException()
        {
            var compra = new Compras
            {
                Id_Proveedor = 1,
                Detalles = new List<DetalleCompra>
        {
            new DetalleCompra
            {
                Id_Producto = 1,
                Cantidad = 2,
                Precio_Unitario = 0
            }
        }
            };

            Assert.Throws<ArgumentException>(() =>
                _compraNegocio.InsertarCompra(compra));
        }
    }
}