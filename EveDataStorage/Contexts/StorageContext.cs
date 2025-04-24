using EveDataStorage.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EveDataStorage.Contexts
{
    public class StorageContext : DbContext
    {
        static StorageContext()
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
        }

        public StorageContext()
        {

        }

        public const string PathDb = "data/storage.db";

        public DbSet<ObservedMining> ObservedMinings => Set<ObservedMining>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<Corporation> Corporations => Set<Corporation>();
        public DbSet<Alliance> Alliances => Set<Alliance>();
        public DbSet<ItemPrice> Prices => Set<ItemPrice>();
        public DbSet<CharacterMain> CharacterMains => Set<CharacterMain>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={PathDb}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Ignore<Company>();
        }

        public static void Migrate()
        {
            using (var context = new StorageContext())
            {
                context.Database.Migrate();
            }
        }

        public static void CreateBackup()
        {
            var backupCount = 10;
            if (File.Exists(PathDb))
            {
                var dirName = Path.GetDirectoryName(PathDb);

                if (!string.IsNullOrEmpty(dirName))
                {
                    var files = Directory.GetFiles(dirName, "*storage_backup*");

                    if (files.Any() && files.Length > backupCount)
                    {
                        var deleteFiles = files.Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).Skip(backupCount).ToList();
                        foreach (var file in deleteFiles)
                        {
                            try
                            {
                                file.Delete();
                            }
                            catch { }
                        }
                    }
                }

                var extName = Path.GetExtension(PathDb);
                var fileName = Path.GetFileNameWithoutExtension(PathDb);
                var newName = $"{fileName}_backup_{DateTime.Now:yyyy.MM.dd_HH.mm.ss}";

                var newFile = Path.Combine(dirName, newName + extName);
                File.Copy(PathDb, newFile);
            }
        }
    }
}
