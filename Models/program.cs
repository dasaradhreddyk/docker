using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConsoleApp.Data;
using ConsoleApp.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// Register DbContext
services.AddDbContext<AAMDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Register Service
services.AddScoped<AAMDataMappingService>();

var serviceProvider = services.BuildServiceProvider();

// Example usage
var service = serviceProvider.GetRequiredService<AAMDataMappingService>();
var result = await service.FetchNewDataAsync("tenant-123");
Console.WriteLine($"Fetched {result.Count} staging records");
