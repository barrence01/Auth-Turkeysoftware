using Auth_Turkeysoftware.Models;
using Microsoft.AspNetCore.Mvc;

namespace Auth_Turkeysoftware.Controllers.Base
{
    [ApiController]
    public class CommonControllerBase : ControllerBase
    {
        /// <summary>
        /// Retorna um <see cref="BadRequestObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status400BadRequest"/>.
        /// </summary>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="BadRequestObjectResult"/> com o status de erro e os dados fornecidos.</returns>
        protected new IActionResult BadRequest(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Data = data
            };
            return base.BadRequest(response);
        }

        /// <summary>
        /// Retorna um <see cref="BadRequestObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status400BadRequest"/>.
        /// </summary>
        /// <param name="message">Mensagem de erro para a resposta.</param>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="BadRequestObjectResult"/> com o status de erro, mensagem e dados fornecidos.</returns>
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

        /// <summary>
        /// Retorna um <see cref="UnauthorizedObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status401Unauthorized"/>.
        /// </summary>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="UnauthorizedObjectResult"/> com o status de erro e os dados fornecidos.</returns>
        protected new IActionResult Unauthorized(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Error,
                Data = data
            };
            return base.Unauthorized(response);
        }

        /// <summary>
        /// Retorna um <see cref="UnauthorizedObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status401Unauthorized"/>.
        /// </summary>
        /// <param name="message">Mensagem de erro para a resposta.</param>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="UnauthorizedObjectResult"/> com o status de erro, mensagem e dados fornecidos.</returns>
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

        /// <summary>
        /// Retorna um <see cref="OkObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status200OK"/>.
        /// </summary>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="OkObjectResult"/> com o status de sucesso e os dados fornecidos.</returns>
        protected new IActionResult Ok(object? data = null)
        {
            var response = new Response
            {
                Status = MessageResponse.Success,
                Data = data
            };
            return base.Ok(response);
        }

        /// <summary>
        /// Retorna um <see cref="OkObjectResult"/> que produz uma resposta <see cref="StatusCodes.Status200OK"/>.
        /// </summary>
        /// <param name="message">Mensagem de sucesso para a resposta.</param>
        /// <param name="data">Dados adicionais para a resposta.</param>
        /// <returns>Um <see cref="OkObjectResult"/> com o status de sucesso, mensagem e dados fornecidos.</returns>
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
