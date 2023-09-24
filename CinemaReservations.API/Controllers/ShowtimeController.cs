using CinemaReservations.Application.Commands.Showtime;
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
    public class ShowtimeController : ControllerBase
    {

        private readonly IMediator _mediatr;
        private readonly IValidator<CreateShowtimeCommand> _validator;

        public ShowtimeController(IMediator mediatr, IValidator<CreateShowtimeCommand> validator)
        {
            _mediatr = mediatr;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShowtime(CreateShowtimeCommand createShowtime)
        {
            var result = new CreateShowtimeResponse();
            try
            {
                var validationResult = await _validator.ValidateAsync(createShowtime);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
                result = await _mediatr.Send(createShowtime);
                return Ok(result.CreateSuccessResponse());
            }
            catch (Exception ex) when (ex.GetType() == typeof(MovieNotExistException) || ex.GetType() == typeof(AuditoriumNotExistException))
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
