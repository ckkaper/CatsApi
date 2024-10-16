using CatsApi.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatsApi.DataAccess
{
    public partial class CatsDbContext : DbContext
    {
        public virtual DbSet<CatEntity> Cat { get; set; }

        public virtual DbSet<TagEntity> Tag { get; set; }

        public CatsDbContext(DbContextOptions<CatsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<CatEntity>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Cats);
        }
    }
}
