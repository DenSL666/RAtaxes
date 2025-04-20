using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using EveDataStorage.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EveDataStorage.Contexts
{
    public class StorageContext : DbContext
    {
        public StorageContext()
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<MiningObserver> Observers => Set<MiningObserver>();
        public DbSet<ObservedMining> ObservedMinings => Set<ObservedMining>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<Corporation> Corporations => Set<Corporation>();
        public DbSet<Alliance> Alliances => Set<Alliance>();
        public DbSet<ItemPrice> Prices => Set<ItemPrice>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=data/storage.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Ignore<Company>();
        }
    }
}
