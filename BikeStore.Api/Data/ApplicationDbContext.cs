using BikeStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Bicicleta> Bicicleta { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Venta> Venta { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Llaves Primarias
            modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Bicicleta>().HasKey(b => b.IdBicicleta);
            modelBuilder.Entity<Cliente>().HasKey(cl => cl.IdCliente);
            modelBuilder.Entity<Venta>().HasKey(v => v.IdVenta);
            modelBuilder.Entity<DetalleVenta>().HasKey(d => d.IdDetalle);

            // Mapeo de la columna calculada Subtotal
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .ValueGeneratedOnAddOrUpdate();

            // Relaciones y Llaves foraneas
            modelBuilder.Entity<Bicicleta>()
                .HasOne(b => b.Categoria)
                .WithMany(c => c.Bicicletas)
                .HasForeignKey(b => b.IdCategoria);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCliente);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.IdVenta);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Bicicleta)
                .WithMany(b => b.DetallesVenta)
                .HasForeignKey(d => d.IdBicicleta);
        }
    }
}
