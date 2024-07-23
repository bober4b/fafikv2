using Fafikv2.Data.Models;
using Fafikv2.Data.DataContext;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Fafikv2.Repositories
{
    public class ServerConfigRepository : IServerConfigRepository
    {
        private readonly DiscordBotDbContext _context;

        public ServerConfigRepository(DiscordBotDbContext context)
        {
            _context =context;
        }

        public Task AddServerConfig(ServerConfig serverConfig)
        {
            _context.ServerConfigs.Add(serverConfig);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
            }
           
            return Task.CompletedTask;
        }

        public Task UpdateServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }

        public Task DeleteServerConfig(ServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServerConfig> GetAll()
        {
            var result = _context.ServerConfigs;
            return result;
        }

        public async Task EnableDisableBans(Guid server, bool enableDisable)
        {
            var config = await _context.ServerConfigs
                .FirstOrDefaultAsync(x => x.ServerId == server)
                .ConfigureAwait(false);
            if (config != null)
            {
                config.BansEnabled=enableDisable;
                try
                {
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task EnableDisableKicks(Guid server, bool enableDisable)
        {
            var config = await _context.ServerConfigs
                .FirstOrDefaultAsync(x => x.ServerId == server)
                .ConfigureAwait(false);
            if (config != null)
            {
                config.KicksEnabled = enableDisable;
                try
                {
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task EnableDisableAutoModerator(Guid server, bool enableDisable)
        {
            var config = await _context.ServerConfigs
                .FirstOrDefaultAsync(x => x.ServerId == server)
                .ConfigureAwait(false);
            if (config != null)
            {
                config.AutoModeratorEnabled = enableDisable;
                try
                {
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task EnableDisableAutoPlay(Guid server, bool enableDisable)
        {
            var config = await _context.ServerConfigs
                .FirstOrDefaultAsync(x => x.ServerId == server)
                .ConfigureAwait(false);
            if (config != null)
            {
                config.AutoplayEnabled = enableDisable;
                try
                {
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
