using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Blazor_TicTac_Mvvm.Data.GameData
{
    /// <summary>
    /// Board data containing the entire super tic tac toe game field.
    /// </summary>
    public class GameBoardData : ObservableObject
    {
        private TicTacState _currentPlayersTurn;
        private TicTacState _gameState;

        public GameBoardData(Guid gameId, TicTacState firstTurnPlayer)
        {
            GameId = gameId;
            CurrentPlayersTurn = firstTurnPlayer;
            Boards = [new(this), new(this), new(this),
                      new(this), new(this) ,new(this),
                      new(this), new(this) ,new(this)];
        }

        /// <summary>
        /// Gets or sets the games guid.
        /// </summary>
        public Guid GameId { get; set; }

        public TicTacState CurrentPlayersTurn
        {
            get => _currentPlayersTurn;
            set => SetProperty(ref _currentPlayersTurn, value);
        }

        /// <summary>
        /// Gets or sets the current state of the full super tic tac toe game.
        /// </summary>
        public TicTacState GameState
        {
            get => _gameState;
            set => SetProperty(ref _gameState, value);
        }

        /// <summary>
        /// The 9 smaller 3x3 tic tac toe fields.
        /// </summary>
        public SmallBoardData[] Boards { get; set; }

        [IgnoreDataMember]
        internal TicTacState LocalPlayer { get; set; }

        [IgnoreDataMember]
        internal bool CurrentlyYourTurn => CurrentPlayersTurn == LocalPlayer;
    }

    /// <summary>
    /// The data for a small tic tac toe board.
    /// </summary>
    public class SmallBoardData : ObservableObject
    {
        private readonly GameBoardData _parent;
        private bool _currentlyActive;
        private TicTacState _smallBoardWon;

        public SmallBoardData(GameBoardData parent, bool currentlyActive = true)
        {
            _parent = parent;
            CurrentlyActive = currentlyActive;
            Fields = [TicTacState.Nobody, TicTacState.Nobody, TicTacState.Nobody,
                      TicTacState.Nobody, TicTacState.Nobody, TicTacState.Nobody,
                      TicTacState.Nobody, TicTacState.Nobody, TicTacState.Nobody];
        }

        /// <summary>
        /// Gets or sets if this smaller instance of the board is currently shown active to select a field to occupie.
        /// </summary>
        public bool CurrentlyActive
        {
            get => _currentlyActive;
            set => SetProperty(ref _currentlyActive, value);
        }

        /// <summary>
        /// Gets or sets if this smaller tic tac toe board, has been won by any of the players or may have drawn.
        /// </summary>
        public TicTacState SmallBoardWon
        {
            get => _smallBoardWon;
            set => SetProperty(ref _smallBoardWon, value);
        }

        /// <summary>
        /// Gets or sets the 9 fields in this tic tac toe area.
        /// </summary>
        public ObservableCollection<TicTacState> Fields { get; set; }

        [IgnoreDataMember]
        internal bool CurrentlyDeactivated => !(CurrentlyActive && _parent.CurrentlyYourTurn);
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
