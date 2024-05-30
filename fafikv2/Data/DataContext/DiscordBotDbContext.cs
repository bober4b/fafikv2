using Fafikv2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Data.DataContext
{
    public class DiscordBotDbContext : DbContext
    {
        private readonly string _conectionstring =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog = DiscordBotDB; Integrated Security = True; Connect Timeout = 30; Encrypt=False;Trust Server Certificate=False;Application Intent = ReadWrite; Multi Subnet Failover=False";
        public DbSet<User> Users { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<ServerConfig> ServerConfigs { get; set; }
        public DbSet<ServerUsers> ServerUsers { get; set; }
        public DbSet<UserServerStats> ServerUsersStats { get; set; }
        public DbSet<BannedWords> BannedWords { get; set; }

       public DiscordBotDbContext() {}

        public DiscordBotDbContext(DbContextOptions<DiscordBotDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerUsers>()
                .HasOne(s => s.UserServerStats)
                .WithOne(su=>su.ServerUsers)
                .HasForeignKey<UserServerStats>(uss=>uss.ServerUserId);
                
                


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
            optionsBuilder.UseSqlServer(_conectionstring);
        }


    }
}
