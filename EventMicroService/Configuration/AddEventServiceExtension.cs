using Microsoft.Extensions.DependencyInjection;
using EventMicroService.Data;
using EventMicroService.Repositories;
using EventMicroService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EventMicroService.Configuration
{
    public static class AddEventServiceExtension
    {
        public static IServiceCollection AddEventServices(this IServiceCollection services, IConfiguration config)
        {
            // 🟢 Nu hämtar vi direkt från "DefaultConnection" (matchar Key Vault)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config["DefaultConnection"]));

            // Repositories
            services.AddScoped<IEventRepository, EventRepository>();

            // Services
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
