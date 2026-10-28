using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;

namespace Capa_Negocio
{
    public class Rol_Negocio
    {
        private readonly string _connectionString;
        private readonly RolDatos _rolDato;

        public Rol_Negocio(IConfiguration configuration)
        {
            // all screens
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));

            _rolDato = new RolDatos(configuration);

        }

        public IEnumerable<Rol> GetRol()
        {
            return _rolDato.GetRol();
        }
    }
}
