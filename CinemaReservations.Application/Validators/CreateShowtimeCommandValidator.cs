using CinemaReservations.Application.Commands.Showtime;
using FluentValidation;

namespace CinemaReservations.Application.Validators
{
    public class CreateShowtimeCommandValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeCommandValidator()
        {
            RuleFor(x => x.AuditoriumId).NotNull();
            RuleFor(x => x.MovieId).NotNull();
            RuleFor(x => x.SessionDate).NotNull();
        }
    }
}
