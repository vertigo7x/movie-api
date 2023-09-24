using CinemaReservations.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CinemaReservations.Services.DataBase
{
    public static class SampleDataExtension
    {
        public static void AddSampleData(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CinemaContext>();
            context.Database.EnsureCreated();


            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 1,
                Showtimes = new List<ShowtimeEntity>
                {
                    new ShowtimeEntity
                    {
                        Id = 1,
                        SessionDate = new DateTime(2023, 1, 1),
                        Movie = new MovieEntity
                        {
                            Id = 1,
                            Title = "Inception",
                            ImdbId = "tt1375666",
                            ReleaseDate = new DateTime(2010, 01, 14),
                            Stars = "Leonardo DiCaprio, Joseph Gordon-Levitt, Ellen Page, Ken Watanabe"
                        },
                    }
                },
                Rows = 28,
                SeatsPerRow = 22,
            });

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 2,
                Rows = 21,
                SeatsPerRow = 18
            });

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 3,
                Rows = 15,
                SeatsPerRow = 21
            });

            context.SaveChanges();
        }
    }
}
