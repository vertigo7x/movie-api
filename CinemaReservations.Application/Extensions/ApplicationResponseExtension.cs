using CinemaReservations.Application.Models;
using System;

namespace CinemaReservations.Application.Extensions
{
    public static class ApplicationResponseExtension
    {
        public static ApplicationResponse CreateSuccessResponse(this ApplicationResponse applicationResponse)
        {
            applicationResponse.EventTime = DateTime.UtcNow;
            applicationResponse.Success = true;
            return applicationResponse;
        }

        public static ApplicationResponse CreateErrorResponse(this ApplicationResponse applicationResponse, string error)
        {
            applicationResponse.EventTime = DateTime.UtcNow;
            applicationResponse.Success = false;
            applicationResponse.Error = error;
            return applicationResponse;
        }
    }
}
