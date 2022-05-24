using LoLShuffler.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LoLShuffler.DAL
{
    public class ShufflerDbContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<Summoner> Summoners { get; set; }

        public ShufflerDbContext(DbContextOptions<ShufflerDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
