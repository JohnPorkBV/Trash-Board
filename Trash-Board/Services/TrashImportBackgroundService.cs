using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrashBoard.Services
{
    public class TrashImportBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

        public TrashImportBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var trashDataService = scope.ServiceProvider.GetRequiredService<ITrashDataService>();
                    var apiTrashDataService = scope.ServiceProvider.GetRequiredService<IApiTrashDataService>();

                    try
                    {
                        Console.WriteLine($" Import From api");
                        await trashDataService.ImportFromApiAsync(apiTrashDataService);

                    }
                    catch (Exception ex)
                    {
                        // Log eventueel de fout
                        Console.WriteLine($"[TrashImportBackgroundService] Import error: {ex.Message}");
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}