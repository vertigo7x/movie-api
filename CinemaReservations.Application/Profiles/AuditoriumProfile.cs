using AutoMapper;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;

namespace CinemaReservations.Application.Profiles
{
    public class AuditoriumProfile : Profile
    {
        public AuditoriumProfile()
        {
            CreateMap<Auditorium, AuditoriumEntity>().ReverseMap();
        }
    }
}
