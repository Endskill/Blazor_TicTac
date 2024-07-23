using Blazor_TicTac_Mvvm.Data.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Blazor_TicTac_Mvvm.Data.Database
{
    public class TicTacContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<SearchGameModel> SearchGames { get; set; }
        public DbSet<SuperTicTacGameModel> Games { get; set; }
        public DbSet<SmallTicTacGameModel> SmallTicTacFields { get; set; }
        public DbSet<GameMoveModel> Moves { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = @"DaTA\Database\SqLite\TicTacDatabase.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().HasMany(user => user.PlayedGamesAsX).WithOne(stttGame => stttGame.PlayerX);
            modelBuilder.Entity<UserModel>().HasMany(user => user.PlayedGamesAsO).WithOne(stttGame => stttGame.PlayerO);

            base.OnModelCreating(modelBuilder);
        }
    }
}
