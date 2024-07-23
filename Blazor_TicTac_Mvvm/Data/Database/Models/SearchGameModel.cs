using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blazor_TicTac_Mvvm.Data.Database.Models
{
    [Table("SearchGame")]
    public class SearchGameModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(HostModel))]
        public int HostPlayerId { get; set; }

        /// <summary>
        /// Gets or sets the time, the host has updated this search.
        /// If the host has not pinged this entry in the last 3 minutes, this match will not be shown anymore.
        /// </summary>
        public DateTime LastHostPing { get; set; }

        /// <summary>
        /// Gets or sets the name of the matchmaking lobby.
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// Gets or sets the connection id, that hosts this match and needs to be messaged when joining.
        /// </summary>
        public string ConnectionIdToMessage { get; set; }

        [ForeignKey(nameof(HostPlayerId))]
        public UserModel HostModel { get; set; }
    }
}
