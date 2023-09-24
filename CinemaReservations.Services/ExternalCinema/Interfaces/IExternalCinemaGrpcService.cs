using ProtoDefinitions;
using System.Threading.Tasks;

namespace CinemaReservations.Services.ExternalCinema.Interfaces
{
    public interface IExternalCinemaGrpcService
    {
        public Task<showListResponse> GetAllMovies();
        public Task<showResponse> GetMovie(string id);
    }
}
