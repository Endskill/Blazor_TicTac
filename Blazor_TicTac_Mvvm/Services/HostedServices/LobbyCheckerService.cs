
using Blazor_TicTac_Mvvm.Data.Database;
using Blazor_TicTac_Mvvm.Hubs;
using Blazor_TicTac_Mvvm.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Blazor_TicTac_Mvvm.Services.HostedServices
{
    public class LobbyCheckerService : BackgroundService
    {
        private readonly IDbContextFactory<TicTacContext> _factory;
        private readonly ILogger<LobbyCheckerService> _logger;
        private readonly IHubContext<LobbySelectorHub, ILobbySelectorClient> _hub;

        public LobbyCheckerService(IDbContextFactory<TicTacContext> factory,
            ILogger<LobbyCheckerService> logger,
            IHubContext<LobbySelectorHub, ILobbySelectorClient> hub)
        {
            _factory = factory;
            _logger = logger;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

            while (!stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync())
            {
                try
                {
                    using (var scope = await _factory.CreateDbContextAsync())
                    {
                        //Find all games, that have not been updated in the last 25 seconds.
                        var oldModels = await scope.SearchGames
                            .Where(model => model.LastHostPing <= DateTime.UtcNow.AddSeconds(-20))
                            .ToListAsync();

                        //When models without a host ping were found, remove them.
                        if (oldModels.Count > 0)
                        {
                            scope.RemoveRange(oldModels);
                            scope.SaveChanges();
                            await _hub.Clients.All.ReceiveMatchClosedAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while executing LobbyCheckerService.");
                }
            }
        }
    }
}
