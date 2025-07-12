using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Api.Models; // Asegúrate de importar tus modelos

namespace InvoiceManagement.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets representan las colecciones de tus entidades que se mapearán a tablas en la base de datos
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceProduct> InvoiceProducts { get; set; }
        public DbSet<CreditNote> CreditNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la relación uno a muchos entre Invoice y InvoiceProduct
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Products)
                .WithOne(ip => ip.Invoice)
                .HasForeignKey(ip => ip.InvoiceNumber)
                .OnDelete(DeleteBehavior.Cascade); // Opcional: Si eliminas una factura, sus productos se eliminan

            // Configurar la relación uno a muchos entre Invoice y CreditNote
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.CreditNotes)
                .WithOne(cn => cn.Invoice)
                .HasForeignKey(cn => cn.InvoiceNumber)
                .OnDelete(DeleteBehavior.Cascade); // Opcional: Si eliminas una factura, sus NCs se eliminan
        }
    }
}