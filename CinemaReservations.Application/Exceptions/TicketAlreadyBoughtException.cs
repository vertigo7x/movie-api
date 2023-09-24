namespace CinemaReservations.Application.Exceptions
{
    public class TicketAlreadyBoughtException : CinemaReservationExceptionBase
    {
        public TicketAlreadyBoughtException(string message) : base(message)
        {
        }
    }
}
