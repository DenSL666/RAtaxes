using EveCommon;
using EveDataStorage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            PathDb = "data\\storage.db";
            //PathDb = DIManager.Configuration.GetValue<string>("ConnectionStrings:eveStorageConnectionString");
            PathDb = Path.Combine(AppContext.BaseDirectory, PathDb);
        }

        public StorageContext()
        {

        }

        public static string PathDb { get; }

        /// <summary>
        /// Коллекция записей о добытой лунной руде.
        /// </summary>
        public DbSet<ObservedMining> ObservedMinings => Set<ObservedMining>();

        /// <summary>
        /// Коллекция персонажей.
        /// </summary>
        public DbSet<Character> Characters => Set<Character>();

        /// <summary>
        /// Коллекция корпораций.
        /// </summary>
        public DbSet<Corporation> Corporations => Set<Corporation>();

        /// <summary>
        /// Коллекция альянсов.
        /// </summary>
        public DbSet<Alliance> Alliances => Set<Alliance>();

        /// <summary>
        /// Коллекция цен на предметы.
        /// </summary>
        public DbSet<ItemPrice> Prices => Set<ItemPrice>();

        /// <summary>
        /// Коллекция пользователей.<br/>
        /// (один пользователь может иметь много персонажей)
        /// </summary>
        public DbSet<CharacterMain> CharacterMains => Set<CharacterMain>();

        /// <summary>
        /// Коллеция транзакций между персонажем и счетом корпорации.
        /// </summary>
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

        /// <summary>
        /// Коллекция типов транзакций. Соотносится с типами транзакций SEAT.
        /// </summary>
        public DbSet<WalletTransactionType> WalletTransactionTypes => Set<WalletTransactionType>();

        /// <summary>
        /// Коллекция регионов, наполненная данными из SDE.
        /// </summary>
        public DbSet<Region> Regions => Set<Region>();

        /// <summary>
        /// Коллекция созвездий, наполненная данными из SDE.
        /// </summary>
        public DbSet<Constellation> Constellations => Set<Constellation>();

        /// <summary>
        /// Коллекция систем, наполненная данными из SDE.
        /// </summary>
        public DbSet<SolarSystem> SolarSystems => Set<SolarSystem>();

        /// <summary>
        /// Коллекция записей о добытых минеральных рудах персонажем.
        /// </summary>
        public DbSet<MineralMining> MineralMinings => Set<MineralMining>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={PathDb}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Ignore<Company>();
        }

        /// <summary>
        /// Выполняет резервное копирование файла текущей БД и затем миграцию текущей БД к обновлённому виду.
        /// </summary>
        public static void Migrate()
        {
            CreateBackup();
            using (var context = new StorageContext())
            {
                context.Database.Migrate();
            }
        }

        /// <summary>
        /// Удаляет более ранние резервные копии.
        /// Выполняет резервное копирование файла текущей БД.
        /// </summary>
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
