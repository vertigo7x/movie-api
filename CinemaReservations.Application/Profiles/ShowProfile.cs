using AutoMapper;
using CinemaReservations.Application.Models;
using ProtoDefinitions;
using System;

namespace CinemaReservations.Application.Profiles
{
    public class ShowProfile : Profile
    {
        public ShowProfile()
        {
            CreateMap<showResponse, Movie>()
                .ForMember(dest => dest.Id, source => source.MapFrom(src => 0))
                .ForMember(dest => dest.Title, source => source.MapFrom(src => src.Title))
                .ForMember(dest => dest.ImdbId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.Stars, source => source.MapFrom(src => src.Crew))
                .ForMember(dest => dest.ReleaseDate, source => source.MapFrom(src => DateTime.Parse($"1/1/{src.Year}")));
        }
    }
}
