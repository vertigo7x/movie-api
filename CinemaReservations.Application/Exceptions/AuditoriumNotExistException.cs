namespace CinemaReservations.Application.Exceptions
{
    public class AuditoriumNotExistException : CinemaReservationExceptionBase
    {
        public AuditoriumNotExistException(string message) : base(message)
        {
        }
    }
}
