using AutoMapper;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;

namespace CinemaReservations.Application.Profiles
{
    public class ShowtimeProfile : Profile
    {
        public ShowtimeProfile()
        {
            CreateMap<Showtime, ShowtimeEntity>().ReverseMap();
        }
    }
}
