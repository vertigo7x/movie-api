using CinemaReservations.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace CinemaReservations.Services.Cache
{
    public class RedisService : IRedisService
    {

        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<string> GetMovie(string id)
        {
            var existingMovie = await _database.StringGetAsync(id);
            return existingMovie.IsNull ? string.Empty : existingMovie.ToString();
        }

        public async Task SetMovie(string id, string jsonObject)
        {
            await _database.StringSetAsync(id, jsonObject);
        }
    }
}
