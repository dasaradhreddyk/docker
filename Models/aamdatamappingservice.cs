vusing System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ConsoleApp.Models;
using ConsoleApp.Data;
using System.Linq;

namespace ConsoleApp.Services
{
    public class AAMDataMappingService
    {
        private readonly StagingDbContext _stagingDbContext;
        private readonly AAMDbContext _aamDbContext;

        public AAMDataMappingService(StagingDbContext stagingDbContext, AAMDbContext aamDbContext)
        {
            _stagingDbContext = stagingDbContext ?? throw new ArgumentNullException(nameof(stagingDbContext));
            _aamDbContext = aamDbContext ?? throw new ArgumentNullException(nameof(aamDbContext));
        }

        /// <summary>
        /// Fetches new data from staging SQL Server
        /// </summary>
        public async Task<List<StagingDataModel>> FetchNewDataAsync(string tenantId)
        {
            try
            {
                var stagingData = await _stagingDbContext.StagingDataModels
                    .Where(x => x.TenantId == tenantId && x.Type == "StagingData")
                    .ToListAsync();

                LogDiagnostics($"Fetched {stagingData.Count} staging records for TenantId: {tenantId}");
                return stagingData;
            }
            catch (DbUpdateException ex)
            {
                LogDiagnostics($"Database error fetching new data: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogDiagnostics($"Error fetching new data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Transforms staging data to AAM data model
        /// </summary>
        public async Task<AAMDataModel> TransformToAAMDataModelAsync(StagingDataModel stagingData, string tenantId)
        {
            var aamDataModel = new AAMDataModel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StagingDataId = stagingData.Id,
                TransformedData = stagingData.Data,
                TransformationTimestamp = DateTime.UtcNow,
                Status = "Transformed"
            };

            return await UpsertAAMItemAsync(aamDataModel);
        }

        /// <summary>
        /// Updates AAM database with transformed data
        /// </summary>
        public async Task<bool> UpdateAAMDbAsync(AAMDataModel aamDataModel, string tenantId)
        {
            try
            {
                _aamDbContext.AAMDataModels.Update(aamDataModel);
                await _aamDbContext.SaveChangesAsync();
                LogDiagnostics($"Updated AAM DB successfully for TenantId: {tenantId}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogDiagnostics($"Error updating AAM DB: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Maps staging data to AAM DB model
        /// </summary>
        public async Task<List<AAMDataModel>> MapStagingDataToAAMDBModelAsync(List<StagingDataModel> stagingDataList, string tenantId)
        {
            var mappedData = new List<AAMDataModel>();

            foreach (var stagingData in stagingDataList)
            {
                var aamModel = await TransformToAAMDataModelAsync(stagingData, tenantId);
                mappedData.Add(aamModel);
            }

            return mappedData;
        }

        /// <summary>
        /// Maps metadata staging data to AAM DB model
        /// </summary>
        public async Task<List<AAMDataModel>> MapMetadataStagingDataToAAMDBModelAsync(List<MetadataStagingModel> metadataList, string tenantId)
        {
            var mappedData = new List<AAMDataModel>();

            foreach (var metadata in metadataList)
            {
                var aamModel = new AAMDataModel
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    MetadataSource = metadata.Source,
                    TransformedData = metadata.MetadataContent,
                    TransformationTimestamp = DateTime.UtcNow,
                    Status = "MetadataMapped"
                };

                var result = await UpsertAAMItemAsync(aamModel);
                mappedData.Add(result);
            }

            return mappedData;
        }

        /// <summary>
        /// Upserts item into AAM SQL Server database
        /// </summary>
        private async Task<T> UpsertAAMItemAsync<T>(T item) where T : class
        {
            try
            {
                var entry = _aamDbContext.Entry(item);

                if (entry.State == EntityState.Detached)
                {
                    _aamDbContext.Set<T>().Add(item);
                }
                else
                {
                    _aamDbContext.Entry(item).State = EntityState.Modified;
                }

                await _aamDbContext.SaveChangesAsync();
                LogDiagnostics($"Upsert successful in AAM DB");
                return item;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("deadlock") ?? false)
            {
                LogDiagnostics($"Deadlock detected, retrying operation...");
                await Task.Delay(1000);
                return await UpsertAAMItemAsync(item);
            }
            catch (Exception ex)
            {
                LogDiagnostics($"Error during upsert: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Logs diagnostic information for monitoring
        /// </summary>
        private void LogDiagnostics(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
