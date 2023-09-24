using CinemaReservations.Application.Commands.ReserveSeats;
using CinemaReservations.Application.Exceptions;
using CinemaReservations.Application.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CinemaReservations.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediatr;
        private readonly IValidator<ReserveSeatsCommand> _validator;

        public ReservationController(IMediator mediatr, IValidator<ReserveSeatsCommand> validator)
        {
            _mediatr = mediatr;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(ReserveSeatsCommand createReservation)
        {
            var result = new ReserveSeatsResponse();
            try
            {
                var validationResult = await _validator.ValidateAsync(createReservation);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
                result = await _mediatr.Send(createReservation);
                return Ok(result.CreateSuccessResponse());
            }
            catch (Exception ex)
                when (ex.GetType() == typeof(AuditoriumNotExistException) || ex.GetType() == typeof(ShowtimeNotExistException))
            {
                return NotFound(result.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(result.CreateErrorResponse(ex.Message));
            }

        }
    }
}
