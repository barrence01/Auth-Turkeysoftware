using Auth_Turkeysoftware.Models;
using Microsoft.AspNetCore.Mvc;

namespace Auth_Turkeysoftware.Controllers.Base
{
    [ApiController]
    public class CommonControllerBase : ControllerBase
    {
        protected new IActionResult BadRequest(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Data = data
            };
            return base.BadRequest(response);
        }

        protected IActionResult BadRequest(string message, object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Message = message,
                Data = data
            };
            return base.BadRequest(response);
        }

        protected new IActionResult Unauthorized(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Data = data
            };
            return base.Unauthorized(response);
        }

        protected IActionResult Unauthorized(string message, object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Message = message,
                Data = data
            };
            return base.Unauthorized(response);
        }

        protected new IActionResult Ok(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Success,
                Data = data
            };
            return base.Ok(response);
        }

        protected IActionResult Ok(string message, object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Success,
                Message = message,
                Data = data
            };
            return base.Ok(response);
        }
    }
}
