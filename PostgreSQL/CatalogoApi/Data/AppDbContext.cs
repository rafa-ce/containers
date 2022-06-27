using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Produto>? Produtos { get; set; }
        public DbSet<Categoria>? Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Categoria>().HasKey(c => c.CategoriaId);
            builder.Entity<Categoria>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
            builder.Entity<Categoria>().Property(c => c.Descricao).HasMaxLength(150).IsRequired();

            builder.Entity<Produto>().HasKey(c => c.ProdutoId);
            builder.Entity<Produto>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
            builder.Entity<Produto>().Property(c => c.Descricao).HasMaxLength(150);
            builder.Entity<Produto>().Property(c => c.Imagem).HasMaxLength(100);
            builder.Entity<Produto>().Property(c => c.Preco).HasPrecision(14, 2);

            builder.Entity<Produto>()
                .HasOne<Categoria>(c => c.Categoria)
                .WithMany(p => p.Produtos)
                .HasForeignKey(c => c.CategoriaId);
        }
    }
}