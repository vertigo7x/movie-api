using System.Threading.Tasks;

namespace CinemaReservations.Services.Cache.Interfaces
{
    public interface IRedisService
    {
        Task<string> GetMovie(string id);
        Task SetMovie(string id, string jsonObject);

    }
}
