namespace Blazor_TicTac_Mvvm.Data.GameData
{
    /// <summary>
    /// Enum to clarify what state a field position or a gamefield is in.
    /// </summary>
    public enum TicTacState
    {
        /// <summary>
        /// This field is not occupied by anyone
        /// </summary>
        Nobody,

        /// <summary>
        /// The game is won or the field position is occupied by the Player X.
        /// </summary>
        X,

        /// <summary>
        /// The game is won or the field position is occupied by the Player O.
        /// </summary>
        O,

        /// <summary>
        /// Used for uncertain game/player states.
        /// </summary>
        Spectator,

        /// <summary>
        /// This game is not won by anyone.
        /// </summary>
        Draw
    }
}
