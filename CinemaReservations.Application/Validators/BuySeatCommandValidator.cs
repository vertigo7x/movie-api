using CinemaReservations.Application.Commands.BuySeats;
using FluentValidation;

namespace CinemaReservations.Application.Validators
{
    public class BuySeatCommandValidator : AbstractValidator<BuySeatCommand>
    {
        public BuySeatCommandValidator()
        {
            RuleFor(x => x.TicketId).NotNull();
        }
    }
}
