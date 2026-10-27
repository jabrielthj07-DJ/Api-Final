using Capa_Datos;
using Capa_Entidades;
using Microsoft.EntityFrameworkCore;

namespace veterinariweb.Context
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):
            base(options) 
        {
            
        }
        // Usuarios
        public DbSet<Usuario> Usuarios { get; set; }

        // Roles
        public DbSet<Rol> Roles { get; set; }

        // Productos
        public DbSet<Producto> Productos { get; set; }

        // Categorias
        public DbSet<Categorias> Categorias { get; set; }

        // Inventarios
        public DbSet<Inventario> Inventarios { get; set; }

    }
}
