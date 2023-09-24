using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaReservations.Services.DataBase
{
    public static class InMemoryDBExtension
    {
        public static void AddInMemoryDB(this IServiceCollection services)
        {
            services.AddScoped<IShowtimesRepository, ShowtimesRepository>();
            services.AddScoped<ITicketsRepository, TicketsRepository>();
            services.AddScoped<IAuditoriumsRepository, AuditoriumsRepository>();

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
        }
    }
}
