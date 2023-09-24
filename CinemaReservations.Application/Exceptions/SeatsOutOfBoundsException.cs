namespace CinemaReservations.Application.Exceptions
{
    public class SeatsOutOfBoundsException : CinemaReservationExceptionBase
    {
        public SeatsOutOfBoundsException(string message) : base(message)
        {
        }
    }
}
