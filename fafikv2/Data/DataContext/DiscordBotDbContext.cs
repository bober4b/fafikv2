using Fafikv2.Data.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace Fafikv2.Data.DataContext
{
    public class DiscordBotDbContext : DbContext,IDisposable
    {
        private readonly string _conectionstring =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog = DiscordBotDB; Integrated Security = True; Connect Timeout = 30; Encrypt=False;Trust Server Certificate=False;Application Intent = ReadWrite; Multi Subnet Failover=False";
        public DbSet<User> Users { get; set; }
       

        public DiscordBotDbContext(DbContextOptions<DiscordBotDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscordBotDbContext).Assembly);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_conectionstring);
        }

        
    }
}
