using AutoMapper;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;

namespace CinemaReservations.Application.Profiles
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<Movie, MovieEntity>().ReverseMap();
        }
    }
}
