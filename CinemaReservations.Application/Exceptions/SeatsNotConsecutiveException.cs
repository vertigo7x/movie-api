namespace CinemaReservations.Application.Exceptions
{
    public class SeatsNotConsecutiveException : CinemaReservationExceptionBase
    {
        public SeatsNotConsecutiveException(string message) : base(message)
        {
        }
    }
}
