using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Blazor_TicTac_Mvvm.Data.Database.Models
{
    [Table("User")]
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }

        [Required]
        public string Name { get; set; }

        public List<SearchGameModel> SearchGames { get; set; }
        public List<SuperTicTacGameModel> PlayedGamesAsX { get; set; }
        public List<SuperTicTacGameModel> PlayedGamesAsO { get; set; }
    }
}
