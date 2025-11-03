using AvanzadaAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace AvanzadaAPI.Data
{
    public class AvanzadaContext : DbContext
    {
        public AvanzadaContext(DbContextOptions<AvanzadaContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<NivelUsuario> NivelesUsuario { get; set; }
        public DbSet<TipoCombustible> TiposCombustible { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<EstadoTurno> EstadosTurno { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public virtual DbSet<TipoServicio> TiposServicio { get; set; }
        public virtual DbSet<Servicio> Servicios { get; set; }
        public virtual DbSet<EstadoServicio> EstadosServicio { get; set; }
        public virtual DbSet<HorariosDisponibles> HorariosDisponibles { get; set; }
        public virtual DbSet<Marca> Marcas { get; set; }
        public virtual DbSet<Modelo> Modelos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(pt => pt.Usuario)
                .WithMany()
                .HasForeignKey(pt => pt.IDUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Modelo -> Marca
            modelBuilder.Entity<Modelo>(entity =>
            {
                entity.HasOne(m => m.Marca)
                      .WithMany() // Una marca puede tener muchos modelos (si no defines la lista en Marca)
                      .HasForeignKey(m => m.IDMarca)
                      .OnDelete(DeleteBehavior.NoAction); // Ojo: No borrar marca si tiene modelos
            });

            // Relación Vehiculo -> Marca y Vehiculo -> Modelo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.HasOne(v => v.Marca)
                      .WithMany()
                      .HasForeignKey(v => v.IDMarca)
                      .OnDelete(DeleteBehavior.NoAction); // No borrar marca si tiene vehículos

                entity.HasOne(v => v.Modelo)
                      .WithMany()
                      .HasForeignKey(v => v.IDModelo)
                      .OnDelete(DeleteBehavior.NoAction); // No borrar modelo si tiene vehículos
            });
        }
    }
}
