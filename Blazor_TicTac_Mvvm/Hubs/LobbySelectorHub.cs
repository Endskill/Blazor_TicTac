using Blazor_TicTac_Mvvm.Data.Database;
using Blazor_TicTac_Mvvm.Data.Database.Models;
using Blazor_TicTac_Mvvm.Data.GameData;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Hubs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace Blazor_TicTac_Mvvm.Hubs
{

    public class LobbySelectorHub : Hub<ILobbySelectorClient>
    {
        /// <summary>
        /// Checks if a game with the name of <paramref name="package"/>.MatchName already exists.
        /// If not creates it adds it onto the database and tells all clients to update their serverlist.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task CreateNewMatchAsync(HostMatchPackage package,
            [FromServices] IDbContextFactory<TicTacContext> factory)
        {
            using (var context = await factory.CreateDbContextAsync())
            {
                //A game already exists and can not be generated.
                if (context.SearchGames.Any(model => model.GameName == package.MatchName))
                {
                    package.CreatedMatch = false;
                    await Clients.Caller.ReceiveMatchOpenedAsync(package);
                    return;
                }

                var searchGameModel = new SearchGameModel()
                {
                    ConnectionIdToMessage = package.ConnId,
                    HostPlayerId = package.HostId,
                    GameName = package.MatchName,
                    LastHostPing = DateTime.UtcNow
                };

                context.Add(searchGameModel);
                context.SaveChanges();

                package.CreatedMatch = true;

                await Clients.All.ReceiveMatchOpenedAsync(package);
            }
        }

        public async Task AnswerJoinRequest(AnswerToJoinRequestPackage package,
            [FromServices] IDbContextFactory<TicTacContext> factory)
        {
            using (var scope = await factory.CreateDbContextAsync())
            {
                var xName = package.hostIsFirstTurn ? package.HostName : package.AskerName;
                var oName = package.hostIsFirstTurn ? package.AskerName : package.HostName;

                var gameModel = new SuperTicTacGameModel()
                {
                    MatchId = package.MatchId,
                    MatchName = package.MatchName,
                    PlayerO = await scope.Users.FirstAsync(model => model.Name == oName),
                    PlayerX = await scope.Users.FirstAsync(model => model.Name == xName),
                    Created = DateTime.UtcNow
                };

                for (int i = 0; i < 9; i++)
                {
                    var smallGameModel = new SmallTicTacGameModel();
                    smallGameModel.MatchId = package.MatchId;
                    smallGameModel.SmallGameIndex = i;
                    smallGameModel.GameState = TicTacState.Nobody;

                    scope.Add(smallGameModel);
                }

                scope.Add(gameModel);
                await scope.SaveChangesAsync();

                await Clients.Clients(new List<string>() { package.HostConnId, package.AskerConnId })
                    .ReceiveAnswerToJoinAsync(package);

                var searchModel = await scope.SearchGames.FirstAsync(model => model.GameName == package.MatchName);
                scope.Remove(searchModel);
                await scope.SaveChangesAsync();

                //TODO HostMatchpackage erstellen um damit die Suche zu beenden.
                var stopSearchPackage = new HostMatchPackage();
                stopSearchPackage.MatchName = package.MatchName;
                stopSearchPackage.CreatedMatch = false;

                await StopSearchingMatch(stopSearchPackage, scope);
            }
        }

        public Task AskToJoinRoom(AskToJoinPackage package)
        {
            return Clients.Client(package.HostConnId).ReceiveSomeoneAskedToJoinAsync(package);
        }

        public Task PingUserAsync(PingPackage pingPackage)
        {
            return Clients.Client(pingPackage.Receiver).ReceivePingRequestAsync(pingPackage);
        }

        public Task AnswerPingAsync(PingPackage pingPackage)
        {
            return Clients.Client(pingPackage.Sender).ReceivePingRequestAsync(pingPackage);
        }

        public async Task StopSearchingMatch(HostMatchPackage package,
            [FromServices] IDbContextFactory<TicTacContext> factory)
        {
            using (var scope = await factory.CreateDbContextAsync())
            {
                await StopSearchingMatch(package, scope);
            }
        }

        private async Task StopSearchingMatch(HostMatchPackage package, TicTacContext scope)
        {
            //Removing old search model.
            var searchModel = await scope.SearchGames.FirstAsync(model => model.GameName == package.MatchName);
            scope.Remove(searchModel);

            await scope.SaveChangesAsync();

            await Clients.All.ReceiveMatchClosedAsync();
        }
    }
}