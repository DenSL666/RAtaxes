using EveCommon;
using EveCommon.Interfaces;
using EveSdeModel;
using EveWebClient.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StaticDataStorage.Contexts;
using StaticDataStorage.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Workers
{
    public class UpdateSdeLogic(IConfig config, ILogger<UpdateSdeLogic> logger, SdeWebHelper sdeWebHelper, IConfiguration configuration)
    {
        protected IConfig Config { get; } = config;
        protected ILogger<UpdateSdeLogic> _logger { get; } = logger;
        protected SdeWebHelper _sdeWebHelper { get; } = sdeWebHelper;
        protected IConfiguration Configuration { get; } = configuration;

        public async Task UpdateSdeDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogWarning("Checking for SDE updates...");

                // Получаем информацию о последней версии
                var versionInfo = await _sdeWebHelper.GetSdeVersion();

                // Проверяем, нужно ли обновление
                if (await IsUpdateRequiredAsync(versionInfo))
                {
                    _logger.LogWarning("New SDE version available: {version}", versionInfo.BuildNumber);

                    // Скачиваем и распаковываем архив
                    var result = await _sdeWebHelper.DownloadSdeArchiveAsync(
                        destinationFolder: configuration.GetValue<string>("Runtime:PathSdeDownload"),
                        cancellationToken: cancellationToken);

                    if (result.Success)
                    {
                        // Дополнительная обработка данных
                        if (await ProcessSdeDataAsync(result.FilePath, cancellationToken))
                        {
                            // Сохраняем информацию о версии
                            await SaveVersionInfoAsync(versionInfo);

                            _logger.LogWarning("SDE updated successfully to version {version}", versionInfo.BuildNumber);
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to update SDE");
                    }
                }
                else
                {
                    //await ProcessSdeDataAsync("Downloads\\eve-online-static-data-latest-yaml.zip", cancellationToken);
                    _logger.LogWarning("SDE is up to date");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SDE update cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SDE update");
            }
        }

        private async Task<bool> IsUpdateRequiredAsync(SdeVersionData newVersion)
        {
            SdeVersionData currentVersion = null;
            using (var context = new StorageContext())
            {
                currentVersion = context.SdeVersions.SingleOrDefault();
            }
            if (currentVersion == null)
            {
                return true;
            }
            else
            {
                return currentVersion.Equals(newVersion) != true;
            }
        }

        private async Task<bool> ProcessSdeDataAsync(string archivePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(archivePath))
                return false;
            var dirPath = Path.GetDirectoryName(archivePath);
            var files = Directory.GetFiles(dirPath);

            try
            {
                StorageContext.Migrate();

                var sdeMain = DIManager.ServiceProvider.GetService<SdeMain>();
                sdeMain.InitBlueprints();
                //sdeMain.InitRegionsAndSolarSystems();

                if (sdeMain.Categories != null && sdeMain.Categories.Any()
                    && sdeMain.Groups != null && sdeMain.Groups.Any())
                {
                    using (var context = new StorageContext())
                    {
                        context.Categories.RemoveRange(context.Categories);
                        context.Groups.RemoveRange(context.Groups);
                        _logger.LogWarning("SDE DB categories, groups cleaned");

                        foreach (var category in sdeMain.Categories)
                        {
                            var newCat = Category.Empty();
                            newCat.FillFrom(category);
                            context.Categories.Add(newCat);
                        }

                        foreach (var group in sdeMain.Groups)
                        {
                            var newGroup = Group.Empty();
                            newGroup.FillFrom(group);
                            context.Groups.Add(newGroup);
                        }
                        context.SaveChanges();
                        _logger.LogWarning("SDE DB categories, groups filled {categories}, {groups}", context.Categories.Count(), context.Groups.Count());
                    }
                }

                if (sdeMain.EntityTypes != null && sdeMain.EntityTypes.Any())
                {
                    using (var context = new StorageContext())
                    {
                        context.EntityTypes.RemoveRange(context.EntityTypes);
                        _logger.LogWarning("SDE DB entityTypes cleaned");

                        foreach (var entityType in sdeMain.EntityTypes)
                        {
                            var newEntity = EntityType.Empty();
                            newEntity.FillFrom(entityType);
                            context.EntityTypes.Add(newEntity);
                        }
                        context.SaveChanges();
                        _logger.LogWarning("SDE DB entityTypes filled {count}", context.EntityTypes.Count());
                    }
                }

                if (sdeMain.TypeMaterials != null && sdeMain.TypeMaterials.Any())
                {
                    using (var context = new StorageContext())
                    {
                        context.TypeMaterials.RemoveRange(context.TypeMaterials);
                        _logger.LogWarning("SDE DB typeMaterials cleaned");

                        foreach (var typeMaterial in sdeMain.TypeMaterials)
                        {
                            var newTypeMaterial = TypeMaterial.Empty();
                            newTypeMaterial.FillFrom(typeMaterial);
                            context.TypeMaterials.Add(newTypeMaterial);
                        }
                        context.SaveChanges();
                        _logger.LogWarning("SDE DB typeMaterials filled {count}", context.TypeMaterials.Count());
                    }
                }

                if (sdeMain.Blueprints != null && sdeMain.Blueprints.Any())
                {
                    using (var context = new StorageContext())
                    {
                        context.Blueprints.RemoveRange(context.Blueprints);
                        _logger.LogWarning("SDE DB blueprints cleaned");

                        foreach (var typeMaterial in sdeMain.Blueprints)
                        {
                            var newBlueprint = Blueprint.Empty();
                            newBlueprint.FillFrom(typeMaterial);
                            context.Blueprints.Add(newBlueprint);
                        }
                        context.SaveChanges();
                        _logger.LogWarning("SDE DB blueprints filled {count}", context.Blueprints.Count());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ProcessSdeDataAsync, path=\"{path}\"", archivePath);
                return false;
            }
            finally
            {
                int k = 0;
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        k++;
                    }
                }
                _logger.LogWarning("SDE deleted extracted files {count}", k);
            }
        }

        private async Task SaveVersionInfoAsync(SdeVersionData versionData)
        {
            try
            {
                using (var context = new StorageContext())
                {
                    if (context.SdeVersions.Count() == 1)
                    {
                        var currentVersion = context.SdeVersions.Single();
                        currentVersion.BuildNumber = versionData.BuildNumber;
                        currentVersion.ReleaseDate = versionData.ReleaseDate;
                    }
                    else
                    {
                        context.SdeVersions.RemoveRange(context.SdeVersions.ToArray());
                        context.SdeVersions.Add(versionData);
                    }
                    context.SaveChanges();
                }

                _logger.LogWarning("Version SDE saved to DB");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save version info");
            }
        }
    }
}
