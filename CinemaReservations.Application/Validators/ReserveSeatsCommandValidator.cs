using CinemaReservations.Application.Commands.ReserveSeats;
using FluentValidation;

namespace CinemaReservations.Application.Validators
{
    public class ReserveSeatsCommandValidator : AbstractValidator<ReserveSeatsCommand>
    {
        public ReserveSeatsCommandValidator()
        {
            RuleFor(x => x.AuditoriumId).NotNull();
            RuleFor(x => x.ShowtimeId).NotNull();
            RuleFor(x => x.Seats).NotNull();
        }
    }
}
