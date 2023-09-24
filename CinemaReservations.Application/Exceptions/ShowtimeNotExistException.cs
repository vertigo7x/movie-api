namespace CinemaReservations.Application.Exceptions
{
    public class ShowtimeNotExistException : CinemaReservationExceptionBase
    {
        public ShowtimeNotExistException(string message) : base(message)
        {
        }
    }
}
