using Fafikv2.Configuration.ConfigJSON;
using Fafikv2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Data.DataContext
{
    public class DiscordBotDbContext : DbContext
    {
        private readonly string _connectionString;
        public DbSet<User> Users { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<ServerConfig> ServerConfigs { get; set; }
        public DbSet<ServerUsers> ServerUsers { get; set; }
        public DbSet<UserServerStats> ServerUsersStats { get; set; }
        public DbSet<BannedWords> BannedWords { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<UserPlayedSong> UserPlayedSongs { get; set; }

        public DiscordBotDbContext()
        {
            JsonReader reader = new();
            reader.ReadJson();
            _connectionString = reader.DbConnection;
        }

        public DiscordBotDbContext(DbContextOptions<DiscordBotDbContext> options) : base(options)
        {

            JsonReader reader = new();
            reader.ReadJson();
            _connectionString = reader.DbConnection;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerUsers>()
                .HasOne(s => s.UserServerStats)
                .WithOne(su => su.ServerUsers)
                .HasForeignKey<UserServerStats>(uss => uss.ServerUserId);

            modelBuilder.Entity<Server>()
                .HasOne(s => s.Config)
                .WithOne(c => c.Server)
                .HasForeignKey<ServerConfig>(c => c.ServerId);

            modelBuilder.Entity<ServerConfig>()
                .HasMany(s => s.BannedWords)
                .WithOne(b => b.ServerConfig)
                .HasForeignKey(b => b.ServerConfigId);

            base.OnModelCreating(modelBuilder);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

    }
}
