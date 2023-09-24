using ApiApplication.Database.Repositories.Abstractions;
using CinemaReservations.Domain.Entities;
using CinemaReservations.Services.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Database.Repositories
{
    public class AuditoriumsRepository : IAuditoriumsRepository
    {
        private readonly CinemaContext _context;

        public AuditoriumsRepository(CinemaContext context)
        {
            _context = context;
        }

        public async Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel)
        {
            return await _context.Auditoriums
                .FirstOrDefaultAsync(x => x.Id == auditoriumId, cancel);
        }
    }
}
