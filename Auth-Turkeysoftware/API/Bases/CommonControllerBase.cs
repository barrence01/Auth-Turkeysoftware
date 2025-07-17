using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Auth_Turkeysoftware.API.Bases
{
    /// <summary>
    /// Classe base para controladores comuns que fornece métodos utilitários para criar respostas HTTP padronizadas.
    /// </summary>
    /// <remarks>
    /// Fornece métodos estáticos para criar respostas consistentes para os status HTTP mais comuns (200, 400, 401).
    /// Todas as respostas seguem o formato padrão da classe <see cref="Response{T}"/>.
    /// </remarks>
    [ApiController]
    public class CommonControllerBase : ControllerBase
    {
        private const string NULL_STRING = null;

        /// <summary>
        /// Cria uma resposta HTTP padronizada com base nos parâmetros fornecidos.
        /// </summary>
        /// <typeparam name="T">Tipo dos dados contidos na resposta.</typeparam>
        /// <param name="status">Código de status HTTP (200, 400, 401, etc.).</param>
        /// <param name="message">Mensagem descritiva sobre a resposta.</param>
        /// <param name="errors">Dicionário de erros onde a chave é o nome do campo e o valor é um array de mensagens de erro.</param>
        /// <param name="payload">Dados opcionais a serem incluídos na resposta.</param>
        /// <returns>Um ObjectResult configurado com a resposta padronizada.</returns>
        private static ObjectResult CreateResponse<T>(int status, string? message, Dictionary<string, string[]>? errors, T? payload)
        {
            var response = new Response<T>(message, errors, payload);
            return status switch
            {
                400 => new BadRequestObjectResult(response),
                401 => new UnauthorizedObjectResult(response),
                200 => new OkObjectResult(response),
                _ => new ObjectResult(response) { StatusCode = status }
            };
        }

        /// <summary>
        /// Retorna uma resposta BadRequest (400) com uma mensagem de erro.
        /// </summary>
        /// <param name="message">Mensagem descritiva do erro.</param>
        /// <returns>Resposta HTTP 400 com a mensagem de erro formatada.</returns>
        protected IActionResult BadRequest(string message) =>
            CreateResponse(StatusCodes.Status400BadRequest, message, ModelState.ToErrorDictionaryOrNull(), NULL_STRING);

        /// <summary>
        /// Retorna uma resposta BadRequest (400) com um objeto de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="payload">Dados a serem incluídos na resposta.</param>
        /// <returns>Resposta HTTP 400 com os dados formatados.</returns>
        protected IActionResult BadRequest<T>(T payload) =>
            CreateResponse(StatusCodes.Status400BadRequest, null, ModelState.ToErrorDictionaryOrNull(), payload);

        /// <summary>
        /// Retorna uma resposta BadRequest (400) com mensagem e objeto de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="message">Mensagem descritiva do erro.</param>
        /// <param name="payload">Dados a serem incluídos na resposta.</param>
        /// <returns>Resposta HTTP 400 com mensagem e dados formatados.</returns>
        protected IActionResult BadRequest<T>(string message, T? payload) =>
            CreateResponse(StatusCodes.Status400BadRequest, message, ModelState.ToErrorDictionaryOrNull(), payload);

        /// <summary>
        /// Retorna uma resposta Unauthorized (401) com uma mensagem de erro.
        /// </summary>
        /// <param name="message">Mensagem descritiva do erro de autenticação/autorização.</param>
        /// <returns>Resposta HTTP 401 com a mensagem formatada.</returns>
        protected IActionResult Unauthorized(string message) =>
            CreateResponse(StatusCodes.Status401Unauthorized, message, ModelState.ToErrorDictionaryOrNull(), NULL_STRING);

        /// <summary>
        /// Retorna uma resposta Unauthorized (401) com um objeto de dados adicional.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="payload">Dados contextuais sobre o erro.</param>
        /// <returns>Resposta HTTP 401 com estrutura de dados formatada.</returns>
        protected IActionResult Unauthorized<T>(T payload) =>
            CreateResponse(StatusCodes.Status401Unauthorized, null, ModelState.ToErrorDictionaryOrNull(), payload);

        /// <summary>
        /// Retorna uma resposta Unauthorized (401) com mensagem e dados contextuais.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="message">Mensagem descritiva do erro.</param>
        /// <param name="payload">Dados adicionais sobre o contexto do erro.</param>
        /// <returns>Resposta HTTP 401 completa com mensagem e dados.</returns>
        protected IActionResult Unauthorized<T>(string message, T? payload) =>
            CreateResponse(StatusCodes.Status401Unauthorized, message, ModelState.ToErrorDictionaryOrNull(), payload);

        /// <summary>
        /// Retorna uma resposta Ok (200) com uma mensagem de sucesso.
        /// </summary>
        /// <param name="message">Mensagem descritiva do sucesso.</param>
        /// <returns>Resposta HTTP 200 com a mensagem formatada.</returns>
        protected static IActionResult Ok(string message) =>
            CreateResponse(StatusCodes.Status200OK, message, null, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta Ok (200) com um objeto de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="payload">Dados a serem retornados na resposta.</param>
        /// <returns>Resposta HTTP 200 com os dados formatados.</returns>
        protected static IActionResult Ok<T>(T payload) =>
            CreateResponse(StatusCodes.Status200OK, null, null, payload);

        /// <summary>
        /// Retorna uma resposta Ok (200) com mensagem e objeto de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de dados.</typeparam>
        /// <param name="message">Mensagem descritiva do sucesso.</param>
        /// <param name="payload">Dados a serem retornados na resposta.</param>
        /// <returns>Resposta HTTP 200 completa com mensagem e dados.</returns>
        protected static IActionResult Ok<T>(string message, T? payload) =>
            CreateResponse(StatusCodes.Status200OK, message, null, payload);
    }
}
