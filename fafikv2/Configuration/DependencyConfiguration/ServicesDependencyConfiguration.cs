using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Fafikv2.Services.dbSevices.Interfaces;
using Fafikv2.Services.dbSevices;

namespace Fafikv2.Configuration.DependencyConfiguration
{
    public static class ServicesDependencyConfiguration
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IServerUsersService, ServerUsersService>();


            return services;
        }
    }
}
