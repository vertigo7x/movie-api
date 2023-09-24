using MediatR;
using System;

namespace CinemaReservations.Application.Commands.BuySeats
{
    public class BuySeatCommand : IRequest<BuySeatResponse>
    {
        public Guid TicketId { get; set; }
    }
}
