using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fafikv2.Data.Models;

namespace Fafikv2.CountSystem
{
    public class LevelSys
    {
        public bool LevelUp(User user)
        {
            var nextlevel=10+user.UserLevel*1.5;
            if(nextlevel<=user.MessagesCountGlobal)
            {
                return true;
            }
            return false;
        }

        public string UserInfo(User user, UserServerStats userServerStats)
        {
            var result = $"Wysłane wiadomości na wszystkich serwerach: {user.MessagesCountGlobal}\n" +
                         $"Wysłane wiadomości na serwerze: {userServerStats.MessagesCountServer}\n" +
                         $"Interakcje z botem na wszystkich serwerach: {user.BotInteractionGlobal}\n" +
                         $"Interakcje z botem na  serwerze: {userServerStats.BotInteractionServer}\n" +
                         $"Poziom: {user.UserLevel}\n" +
                         $"Następny poziom: {user.MessagesCountGlobal/(10+user.UserLevel*1.5)}\n" +
                         $"Karma globalna: {user.GlobalKarma} \n" +
                         $"Karma serwera: {userServerStats.ServerKarma}";
                         

            return result;
        }


    }
}

