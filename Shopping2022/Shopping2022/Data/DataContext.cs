using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data.Entities;

namespace Shopping2022.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<TemporalSale> TemporalSales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //        modelBuilder.Entity<Cae>()
            //.HasIndex(p => new { p.Empresa, p.CAETipoCFE, p.CAENumero })
            //.IsUnique();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique();

            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

            modelBuilder.Entity<State>().HasIndex("Name", "CountryId").IsUnique();

            modelBuilder.Entity<City>().HasIndex("Name", "StateId").IsUnique();

            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<ProductCategory>().HasIndex("ProductId", "CategoryId").IsUnique();

            //modelBuilder.Entity<State>().HasIndex(s => new { s.Name, s.Id }).IsUnique();
            //modelBuilder.Entity<City>().HasIndex(c => new { c.Name, c.Id }).IsUnique();

        }
    }
}
