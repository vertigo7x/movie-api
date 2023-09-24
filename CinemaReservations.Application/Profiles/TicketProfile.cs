using AutoMapper;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;

namespace CinemaReservations.Application.Profiles
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<Ticket, TicketEntity>().ReverseMap();
        }
    }
}
