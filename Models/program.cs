using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConsoleApp.Data;
using ConsoleApp.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// Register Staging DbContext
services.AddDbContext<StagingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("StagingConnection")));

// Register AAM DbContext
services.AddDbContext<AAMDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AAMConnection")));

// Register Service
services.AddScoped<AAMDataMappingService>();

var serviceProvider = services.BuildServiceProvider();

// Example usage
var service = serviceProvider.GetRequiredService<AAMDataMappingService>();
var stagingData = await service.FetchNewDataAsync("tenant-123");
var mappedData = await service.MapStagingDataToAAMDBModelAsync(stagingData, "tenant-123");
Console.WriteLine($"Mapped {mappedData.Count} records to AAM DB");
