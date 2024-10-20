using CatsApi.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatsApi.DataAccess
{
    public class CatsDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public virtual DbSet<CatEntity> Cat { get; set; }

        public virtual DbSet<TagEntity> Tag { get; set; }

        public CatsDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var b = _configuration.GetConnectionString("DefaultConnection");
            //var a = "Server=tcp:127.0.0.1,1433;Initial Catalog=natech;Persist Security Info=False;User ID=SA;Password=;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;";
            optionsBuilder.
                UseSqlServer(connectionString: b);
            //UseSqlServer(connectionString: b, (options) => options.EnableRetryOnFailure());
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatEntity>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Cats);
        }
    }
}
