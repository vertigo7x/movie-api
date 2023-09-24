using System;

namespace CinemaReservations.Application.Exceptions
{
    public class CinemaReservationExceptionBase : Exception
    {
        public CinemaReservationExceptionBase()
        {
        }

        public CinemaReservationExceptionBase(string message)
            : base(message)
        {
        }

        public CinemaReservationExceptionBase(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
