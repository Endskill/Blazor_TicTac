using Blazor_TicTac_Mvvm.Data.GameData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazor_TicTac_Mvvm.Data.Database.Models
{
    [Table("GameMove")]
    public class GameMoveModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(GameModel))]
        [Required]
        public Guid GameId { get; set; }

        [ForeignKey(nameof(SmallTicTacGame))]
        [Required]
        public int SmallTicTacId { get; set; }

        /// <summary>
        /// The count 
        /// </summary>
        public int MoveCounter { get; set; }

        /// <summary>
        /// The index at which smaller tic tac toe game you've placed a X or O.
        /// </summary>
        /// <example>
        /// Index = 2 would be:
        /// O O X
        /// O O O
        /// O O O
        /// </example>
        public int FieldIndex { get; set; }

        public TicTacState FieldState { get; set; }

        [ForeignKey(nameof(SmallTicTacId))]
        public SmallTicTacGameModel SmallTicTacGame { get; set; }

        [ForeignKey(nameof(GameId))]
        public SuperTicTacGameModel GameModel { get; set; }
    }
}
