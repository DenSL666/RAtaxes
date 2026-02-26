using EveCommon;
using StaticDataStorage.Models;
using StaticDataStorage.Models.Celestial;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StaticDataStorage.Contexts
{
    public class StorageContext : DbContext
    {
        static StorageContext()
        {
            PathDb = "data\\static_data_storage.db";
            //PathDb = DIManager.Configuration.GetValue<string>("ConnectionStrings:eveStorageConnectionString");
            PathDb = Path.Combine(AppContext.BaseDirectory, PathDb);
        }

        public StorageContext()
        {
            Database.EnsureCreated();
        }

        public static string PathDb { get; }

        /// <summary>
        /// Данные об используемой версии SDE.
        /// </summary>
        public DbSet<SdeVersionData> SdeVersions => Set<SdeVersionData>();

        /// <summary>
        /// Коллекция игровых чертежей.
        /// </summary>
        public DbSet<Blueprint> Blueprints => Set<Blueprint>();

        /// <summary>
        /// Коллекция категорий сущностей.
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Коллекция групп сущностей.
        /// </summary>
        public DbSet<Group> Groups => Set<Group>();

        /// <summary>
        /// Коллекция игровых сущностей.
        /// </summary>
        public DbSet<EntityType> EntityTypes => Set<EntityType>();

        /// <summary>
        /// Коллекция данных, на какие сущности могут быть переработаны игровые сущности.
        /// </summary>
        public DbSet<TypeMaterial> TypeMaterials => Set<TypeMaterial>();


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
        /// Коллекция планет, наполненная данными из SDE.
        /// </summary>
        public DbSet<Planet> Planets => Set<Planet>();

        /// <summary>
        /// Коллекция лун, наполненная данными из SDE.
        /// </summary>
        public DbSet<Moon> Moons => Set<Moon>();

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
                    var files = Directory.GetFiles(dirName, "*static_data_storage_backup*");

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
