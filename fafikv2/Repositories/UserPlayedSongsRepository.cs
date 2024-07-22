﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Fafikv2.Data.DataContext;
using Fafikv2.Data.Models;
using Fafikv2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fafikv2.Repositories
{
    public class UserPlayedSongsRepository : IUserPlayedSongsRepository
    {
        private readonly DiscordBotDbContext _context;

        public UserPlayedSongsRepository(DiscordBotDbContext context)
        {
            _context = context;
        }
        public async Task Add(UserPlayedSong userPlayedSong)
        {
           await _context.UserPlayedSongs.AddAsync(userPlayedSong).ConfigureAwait(false);

           await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<bool> HasBeenAdded(UserPlayedSong userPlayedSong)
        {
            var result=await _context.UserPlayedSongs
                .AnyAsync(x=>x.Song==userPlayedSong.Song && x.User==userPlayedSong.User)
                .ConfigureAwait(false);
            return result;
        }
    }
}
