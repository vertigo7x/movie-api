using CinemaReservations.Services.ExternalCinema.Interfaces;
using ProtoDefinitions;
using System.Threading.Tasks;

namespace CinemaReservations.Services.ExternalCinema
{
    public class ExternalCinemaGrpcService : IExternalCinemaGrpcService
    {
        private readonly MoviesApi.MoviesApiClient _client;

        public ExternalCinemaGrpcService(MoviesApi.MoviesApiClient client)
        {
            _client = client;
        }

        public async Task<showListResponse> GetAllMovies()
        {
            var all = await _client.GetAllAsync(new Empty());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showResponse> GetMovie(string id)
        {
            IdRequest idRequest = new IdRequest()
            {
                Id = id
            };
            var all = await _client.GetByIdAsync(idRequest);
            all.Data.TryUnpack<showResponse>(out var data);
            return data;
        }
    }
}
