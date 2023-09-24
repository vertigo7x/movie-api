using AutoMapper;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;

namespace CinemaReservations.Application.Profiles
{
    public class SeatProfile : Profile
    {
        public SeatProfile()
        {
            CreateMap<Seat, SeatEntity>().ReverseMap();
        }
    }
}
