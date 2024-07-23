using Blazor_TicTac_Mvvm.Data.GameData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazor_TicTac_Mvvm.Data.Database.Models
{
    [Table("SmallTicTacGame")]
    public class SmallTicTacGameModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(SuperTicTacGame))]
        public Guid MatchId { get; set; }

        /// <summary>
        /// The index this smaller tic tac toe game is positioned inside of the <see cref="SuperTicTacGame"/>.
        /// </summary>
        /// <example>
        /// Index = 2 would be:
        /// O O X
        /// O O O
        /// O O O
        /// </example>
        public int SmallGameIndex { get; set; }

        /// <summary>
        /// Gets or sets the state of this smaller tic tac toe game.
        /// </summary>
        public TicTacState GameState { get; set; }

        /// <summary>
        /// All moves that were set.
        /// </summary>
        public List<GameMoveModel> Moves { get; set; }

        [ForeignKey(nameof(MatchId))]
        public SuperTicTacGameModel SuperTicTacGame { get; set; }
    }
}
