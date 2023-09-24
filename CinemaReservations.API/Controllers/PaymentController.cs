using CinemaReservations.Application.Commands.BuySeats;
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
    public class PaymentController : ControllerBase
    {

        private readonly IMediator _mediatr;
        private readonly IValidator<BuySeatCommand> _validator;

        public PaymentController(IMediator mediatr, IValidator<BuySeatCommand> validator)
        {
            _mediatr = mediatr;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(BuySeatCommand buySeat)
        {
            var result = new BuySeatResponse();
            try
            {
                var validationResult = await _validator.ValidateAsync(buySeat);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
                result = await _mediatr.Send(buySeat);
                return Ok(result.CreateSuccessResponse());
            }
            catch (TicketNotExistException ex)
            {
                return NotFound(result.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex) when (ex.GetType() != typeof(TicketNotExistException))
            {
                return BadRequest(result.CreateErrorResponse(ex.Message));
            }
        }
    }
}
