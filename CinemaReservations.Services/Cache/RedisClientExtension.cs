using CinemaReservations.Services.Cache.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CinemaReservations.Services.Cache
{
    public static class RedisClientExtension
    {
        public static void AddRedisClient(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis"];
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<IRedisService, RedisService>();
        }
    }
}
