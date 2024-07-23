using Blazor_TicTac_Mvvm.Data.GameData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazor_TicTac_Mvvm.Data.Database.Models
{
    [Table("SuperTicTacGame")]
    public class SuperTicTacGameModel
    {
        [Required]
        [Key]
        public Guid MatchId { get; set; }

        [Required]
        public string MatchName { get; set; }

        [Required]
        [ForeignKey(nameof(PlayerX))]
        public int PlayerX_Id { get; set; }

        [Required]
        [ForeignKey(nameof(PlayerO))]
        public int PlayerO_Id { get; set; }

        /// <summary>
        /// Gets or sets when this game of super tic tac toe was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the current state of the complete super tic tac toe game.
        /// </summary>
        public TicTacState SuperGameState { get; set; }

        [ForeignKey(nameof(PlayerX_Id))]
        public UserModel PlayerX { get; set; }

        [ForeignKey(nameof(PlayerO_Id))]
        public UserModel PlayerO { get; set; }

        public List<GameMoveModel> GameMoves { get; set; }
        public List<SmallTicTacGameModel> SmallTicTacGames { get; set; }
    }
}
