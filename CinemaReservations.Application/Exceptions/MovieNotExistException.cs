namespace CinemaReservations.Application.Exceptions
{
    public class MovieNotExistException : CinemaReservationExceptionBase
    {
        public MovieNotExistException(string message) : base(message)
        {
        }
    }
}