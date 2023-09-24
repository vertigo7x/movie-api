namespace CinemaReservations.Application.Exceptions
{
    public class TicketNotExistException : CinemaReservationExceptionBase
    {
        public TicketNotExistException(string message) : base(message)
        {
        }
    }
}
