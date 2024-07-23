using Blazor_TicTac_Mvvm.Data.Database;
using Blazor_TicTac_Mvvm.Data.Database.Models;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace Blazor_TicTac_Mvvm.Services
{
    public class DatabaseService
    {
        private readonly IDbContextFactory<TicTacContext> _factory;

        public DatabaseService(IDbContextFactory<TicTacContext> factory)
        {
            _factory = factory;
        }

        public async Task<UserModel?> QueryUser(string userName)
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                return await scope.Users.FirstOrDefaultAsync(x => x.Name == userName);
            }
        }

        public async Task<UserModel> CreateUser(string userName)
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                var userModel = new UserModel()
                {
                    Name = userName
                };

                scope.Users.Add(userModel);
                await scope.SaveChangesAsync();

                return userModel;
            }
        }

        public async Task<List<SuperTicTacGameModel>> GetPlayedGamesAsync(int playerId)
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                return await scope.Games
                    .Include(model => model.PlayerX)
                    .Include(model => model.PlayerO)
                    .Where(model => model.PlayerX_Id == playerId || model.PlayerO_Id == playerId)
                    .ToListAsync();
            }
        }

        public async Task<List<SearchGameModel>> GetOpenGames()
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                return await scope.SearchGames.Include(model => model.HostModel).Where(model => model.LastHostPing > DateTime.UtcNow.AddMinutes(-30)).ToListAsync();
            }
        }

        public Task<SuperTicTacGameModel> CreateTicTacGameAsync()
        {
            using (var scope = _factory.CreateDbContext())
            {
                return CreateTicTacGameAsync(scope);
            }
        }

        public void KeepSearchGameAlive(int hostId)
        {
            using (var scope = _factory.CreateDbContext())
            {
                var game = scope.SearchGames.FirstOrDefault(model => model.HostPlayerId == hostId);
                if(game != null)
                {
                    game.LastHostPing = DateTime.UtcNow;
                    scope.SaveChanges();
                }
            }
        }

        internal async Task<SuperTicTacGameModel> CreateTicTacGameAsync(TicTacContext scope)
        {
            return null;
        }

        internal async Task Debug_RestartDatabaseAsync()
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                await scope.Database.EnsureDeletedAsync();
                await scope.Database.EnsureCreatedAsync();

                var testUser = new UserModel()
                {
                    Name = "tobi"
                };
                scope.Users.Add(testUser);
                await scope.SaveChangesAsync();

            }

            using (var testScope = await _factory.CreateDbContextAsync())
            {
                var testUser = new UserModel()
                {
                    Name = "tobi2"
                };
                testScope.Users.Add(testUser);
                await testScope.SaveChangesAsync();
            }
        }

        public async Task<SuperTicTacGameModel> QueryGame(Guid guid)
        {
            using (var scope = await _factory.CreateDbContextAsync())
            {
                return await scope.Games
                    .Include(model => model.GameMoves)
                    .Include(model => model.SmallTicTacGames)
                    .ThenInclude(smallGameModel => smallGameModel.Moves)
                    .FirstOrDefaultAsync(model => model.MatchId == guid);
            }
        }
    }
}
