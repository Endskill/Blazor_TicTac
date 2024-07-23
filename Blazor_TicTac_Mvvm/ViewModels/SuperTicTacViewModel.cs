using Blazor_TicTac_Mvvm.Base;
using Blazor_TicTac_Mvvm.Data.Database.Models;
using Blazor_TicTac_Mvvm.Data.GameData;
using Blazor_TicTac_Mvvm.Hubs.Clients;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Blazor_TicTac_Mvvm.ViewModels
{
    public class SuperTicTacViewModel : ViewModelBase, IDisposable
    {
        private readonly DatabaseService _dbService;
        private bool _currentlyYourTurn;
        private GameBoardData _boardData;
        private readonly GameClient _client;
        private readonly IDialogService _dialogService;
        private readonly NavigationManager _navManager;

        public SuperTicTacViewModel(DatabaseService dbService, GameClient client,
            IDialogService dialogService, NavigationManager navManager)
        {
            _dbService = dbService;
            _client = client;
            _dialogService = dialogService;
            _navManager = navManager;

            _client.SetField = new EventCallback<SetFieldPackage>(null, ReceiveSetField);
            _client.IllegalSetField = new EventCallback(null, ReceiveIllegalMove);
            _client.GameWon = new EventCallback(null, ReceiveGameWonAsync);
        }

        public bool CurrentlyYourTurn
        {
            get => _currentlyYourTurn;
            set => SetProperty(ref _currentlyYourTurn, value);
        }

        public Guid MatchId { get; internal set; }
        public GameBoardData BoardData
        {
            get => _boardData;
            set => SetProperty(ref _boardData, value);
        }
        public Task<AuthenticationState> AuthenticationState { get; internal set; }

        public override async Task OnInitializedAsync()
        {
            var game = await _dbService.QueryGame(MatchId);
            var user = await AuthenticationState;
            var identity = user.User.Identities.FirstOrDefault();
            var localPlayerId = "";
            if (identity != null)
                localPlayerId = identity.Claims.First(x => x.Type == System.Security.Claims.ClaimTypes.Sid).Value;

            var board = new GameBoardData(MatchId, TicTacState.X);

            if (game.PlayerX_Id.ToString() == localPlayerId)
                board.LocalPlayer = TicTacState.X;
            else if (game.PlayerO_Id.ToString() == localPlayerId)
                board.LocalPlayer = TicTacState.O;
            else
                board.LocalPlayer = TicTacState.Nobody;

            BoardData = board;

            if (game.GameMoves.Count > 0)
                RedoMoves(game);

            await _client.SetupGameAsync(MatchId);

            await base.OnInitializedAsync();
        }

        public async Task PlaceFieldAsync((int gameFieldIndex, int fieldIndex) data)
        {
            BoardData.CurrentPlayersTurn = TicTacState.Nobody;
            NotifyStateChanged();

            var package = new SetFieldPackage();
            package.MatchId = MatchId;
            package.GameFieldIndex = data.gameFieldIndex;
            package.FieldIndex = data.fieldIndex;
            package.Player = BoardData.LocalPlayer;

            await _client.SetFieldAsync(package);
        }

        public void ReceiveSetField(SetFieldPackage package)
        {
            BoardData.Boards[package.GameFieldIndex].OccupieField(package.FieldIndex, package.Player);
            UnlockBoard(package.FieldIndex);
            BoardData.CurrentPlayersTurn = package.Player == TicTacState.X ? TicTacState.O : TicTacState.X;

            NotifyStateChanged();
        }

        public void ReceiveIllegalMove()
        {
            if(BoardData.CurrentPlayersTurn == TicTacState.Nobody)
            {
                BoardData.CurrentPlayersTurn = BoardData.LocalPlayer;
                NotifyStateChanged();
            }
        }

        public Task ReceiveGameWonAsync(TicTacState winner)
        {
            return InvokeAsync(async () =>
            {
               await _dialogService.ShowMessageBox("Game Won",
                    $"The game was won by player {(winner)}");

                _navManager.NavigateTo("/lobbyselector");
            });
        }

        private void RedoMoves(SuperTicTacGameModel model)
        {
            foreach (var moves in model.GameMoves)
            {
                BoardData.Boards[moves.SmallTicTacGame.SmallGameIndex].OccupieField(moves.FieldIndex, moves.FieldState);
            }

            var lastMove = model.GameMoves.OrderByDescending(moves => moves.MoveCounter).FirstOrDefault();
            BoardData.CurrentPlayersTurn = lastMove.FieldState == TicTacState.X ? TicTacState.O : TicTacState.X;

            UnlockBoard(lastMove.FieldIndex);

            NotifyStateChanged();
        }

        private void UnlockBoard(int smallBoardIndex)
        {
            if (BoardData.Boards[smallBoardIndex].SmallBoardWon != TicTacState.Nobody)
            {
                smallBoardIndex = -1;
            }

            for (int i = 0; i < BoardData.Boards.Length; i++)
            {
                BoardData.Boards[i].CurrentlyActive = (smallBoardIndex == -1 || smallBoardIndex == i);
            }
        }

        //When a client disconnects, the server side of this viewmodel runs dispose, might 
        public void Dispose()
        {
        }
    }

    public static class BoardExtensions
    {
        public static bool OccupieField(this SmallBoardData board, int fieldIndex, TicTacState player)
        {
            if (board.Fields[fieldIndex] != TicTacState.Nobody)
            {
                throw new Exception($"The field at position {fieldIndex} is already occupied!");
            }

            board.Fields[fieldIndex] = player;

            board.SmallBoardWon = board.CheckForWin();

            //Return if the board won has been changed.
            return board.SmallBoardWon != TicTacState.Nobody;
        }

        private static TicTacState CheckForWin(this SmallBoardData board)
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
                { 2, 4, 6 },
            };

            for (int i = 0; i < 8; i++)
            {
                if (board.Fields[winningCombos[i, 0]] != TicTacState.Nobody
                    && board.Fields[winningCombos[i, 0]] == board.Fields[winningCombos[i, 1]]
                    && board.Fields[winningCombos[i, 0]] == board.Fields[winningCombos[i, 2]])
                {
                    return board.Fields[winningCombos[i, 0]];
                }
            }

            bool fullBoard = true;
            for (int i = 0; i < 9; i++)
            {
                if (fullBoard && board.Fields[i] == TicTacState.Nobody)
                    fullBoard = false;
            }

            if (fullBoard)
                return TicTacState.Draw;

            return TicTacState.Nobody;
        }
    }
}
