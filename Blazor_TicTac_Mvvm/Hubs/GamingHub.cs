using Blazor_TicTac_Mvvm.Data.Database;
using Blazor_TicTac_Mvvm.Data.Database.Models;
using Blazor_TicTac_Mvvm.Data.GameData;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Hubs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Blazor_TicTac_Mvvm.Hubs
{
    public class GamingHub : Hub<IGameClient>
    {
        public Task JoinGroupAsync(Guid matchId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
        }

        public async Task SetFieldAsync(SetFieldPackage package,
            [FromServices] IDbContextFactory<TicTacContext> factory)
        {
            using (var scope = await factory.CreateDbContextAsync())
            {
                var game = await scope.Games
                    .Include(model => model.SmallTicTacGames)
                    .ThenInclude(smallGameModel => smallGameModel.Moves)
                    .Include(model => model.GameMoves)
                    .FirstAsync(model => model.MatchId == package.MatchId);

                if(game.SuperGameState != TicTacState.Nobody)
                {
                    await Clients.Group(package.MatchId.ToString()).ReceiveIllegalFieldSetSync(package);
                    return;
                }

                var lastMove = game.GameMoves
                    .OrderByDescending(model => model.MoveCounter)
                    .FirstOrDefault();

                int currentMoveCount = 0;

                if (lastMove != null)
                {
                    currentMoveCount = lastMove.MoveCounter + 1;

                    //No 2 moves in a row!
                    if (lastMove.FieldState == package.Player)
                    {
                        await Clients.Group(package.MatchId.ToString()).ReceiveIllegalFieldSetSync(package);
                        return;
                    }

                    //Getting the game, we must place our move right now.
                    var aimedSmallerGame = game.SmallTicTacGames.First(model => model.SmallGameIndex == lastMove.FieldIndex);
                    //Checking if there is free space in the aimed smaller game.
                    if (aimedSmallerGame.GameState == TicTacState.Nobody)
                    {
                        //If we try to set a field at any other game field index -> NO
                        if (package.GameFieldIndex != aimedSmallerGame.SmallGameIndex)
                        {
                            await Clients.Group(package.MatchId.ToString()).ReceiveIllegalFieldSetSync(package);
                            return;
                        }
                    }
                }
                //When there was no move yet, checking if X has placed yet.
                else if (package.Player != TicTacState.X)
                {
                    await Clients.Group(package.MatchId.ToString()).ReceiveIllegalFieldSetSync(package);
                    return;
                }

                //All good place field for real.
                var moveModel = new GameMoveModel();
                moveModel.GameModel = game;
                moveModel.SmallTicTacGame = game.SmallTicTacGames
                    .First(model => model.SmallGameIndex == package.GameFieldIndex);
                moveModel.MoveCounter = currentMoveCount;
                moveModel.FieldIndex = package.FieldIndex;
                moveModel.FieldState = package.Player;

                //Setting the new small tic tac game game state, by checking if there is a winning combination.
                moveModel.SmallTicTacGame.Moves.Add(moveModel);
                moveModel.SmallTicTacGame.GameState = CheckSmallTicTacState(moveModel.SmallTicTacGame.Moves);

                if(moveModel.SmallTicTacGame.GameState != TicTacState.Nobody)
                {
                    game.SuperGameState = CheckOverallWinner(game);
                }

                scope.Add(moveModel);
                await scope.SaveChangesAsync();

                await Clients.Group(package.MatchId.ToString()).ReceiveSetFieldAsync(package);

                if (game.SuperGameState != TicTacState.Nobody)
                    await Clients.Group(package.MatchId.ToString()).ReceiveWinnerAsync(game.SuperGameState);
            }
        }

        private TicTacState CheckSmallTicTacState(List<GameMoveModel> moves)
        {
            var winningCombos = new int[8, 3]
            {
                { 0, 1, 2 },
                { 3, 4, 5 },
                { 6, 7, 8 },
                { 0, 3, 6 },
                { 1, 4, 7 },
                { 2, 5, 8 },
                { 0, 4, 8 },
                { 2, 4, 6 }
            };

            for (int i = 0; i < 8; i++)
            {
                var pos1 = moves.FirstOrDefault(move => move.FieldIndex == winningCombos[i, 0])?.FieldState;
                if (pos1 != null
                    && moves.FirstOrDefault(move => move.FieldIndex == winningCombos[i, 1])?.FieldState == pos1
                    && moves.FirstOrDefault(move => move.FieldIndex == winningCombos[i, 2])?.FieldState == pos1)
                {
                    return (TicTacState)pos1;
                }
            }

            if (moves.Count == 9)
                return TicTacState.Draw;

            return TicTacState.Nobody;
        }

        private TicTacState CheckOverallWinner(SuperTicTacGameModel game)
        {
            var winningCombos = new int[8, 3]
           {
                { 0, 1, 2 },
                { 3, 4, 5 },
                { 6, 7, 8 },
                { 0, 3, 6 },
                { 1, 4, 7 },
                { 2, 5, 8 },
                { 0, 4, 8 },
                { 2, 4, 6 }
           };

            for (int i = 0; i < 8; i++)
            {
                var boardState = game.SmallTicTacGames.FirstOrDefault(move => move.SmallGameIndex == winningCombos[i, 0])?.GameState;
                if (boardState != null && boardState != TicTacState.Nobody
                    && game.SmallTicTacGames.FirstOrDefault(move => move.SmallGameIndex == winningCombos[i, 1])?.GameState == boardState
                    && game.SmallTicTacGames.FirstOrDefault(move => move.SmallGameIndex == winningCombos[i, 2])?.GameState == boardState)
                {
                    return boardState ?? throw new NullReferenceException("compiler bug");
                }
            }

            return TicTacState.Nobody;
        }
    }
}