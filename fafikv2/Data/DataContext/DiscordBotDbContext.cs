using Fafikv2.Data.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace Fafikv2.Data.DataContext
{
    public class DiscordBotDbContext : DbContext,IDisposable
    {
        private readonly string conectionstring =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog = DiscordBotDB; Integrated Security = True; Connect Timeout = 30; Encrypt=False;Trust Server Certificate=False;Application Intent = ReadWrite; Multi Subnet Failover=False";

        public DiscordBotDbContext()
        {

        }

        public DiscordBotDbContext(DbContextOptions options) : base(options) { }


        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(conectionstring);
        }

        
    }
}
