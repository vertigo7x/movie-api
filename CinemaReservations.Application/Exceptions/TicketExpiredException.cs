namespace CinemaReservations.Application.Exceptions
{
    public class TicketExpiredException : CinemaReservationExceptionBase
    {
        public TicketExpiredException(string message) : base(message)
        {
        }
    }
}
