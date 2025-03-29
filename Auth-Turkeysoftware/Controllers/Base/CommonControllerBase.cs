using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Auth_Turkeysoftware.Controllers.Base
{
    /// <summary>
    /// Classe base para controladores comuns que fornece métodos utilitários para criar respostas HTTP padronizadas.
    /// </summary>
    [ApiController]
    public class CommonControllerBase : ControllerBase
    {
        private const string SUCCESS = "Success";
        private const string ERROR = "Error";
        private const string NULL_STRING = null;
        private const int STATUS_400 = StatusCodes.Status400BadRequest;
        private const int STATUS_401 = StatusCodes.Status401Unauthorized;
        private const int STATUS_200 = StatusCodes.Status200OK;
        private const string UNAUTHORIZED = "Unauthorized";
        private const string BAD_REQUEST = "BadRequest";

        private static readonly Lazy<List<string>> _emptyStringList = new Lazy<List<string>>(() => new List<string>(0));
        private static List<string> EmptyStringList => _emptyStringList.Value;

        /// <summary>
        /// Cria uma resposta HTTP com base nos parâmetros fornecidos.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="status">Código de status HTTP.</param>
        /// <param name="title">Título da resposta.</param>
        /// <param name="message">Mensagem da resposta.</param>
        /// <param name="errors">Lista de erros.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto ObjectResult com a resposta HTTP.</returns>
        private static ObjectResult CreateResponse<T>(int status, string title, string? message, List<string>? errors, T? data)
        {
            var response = new Response<T>(status, title, message, errors, data);
            return status switch
            {
                400 => new BadRequestObjectResult(response),
                401 => new UnauthorizedObjectResult(response),
                200 => new OkObjectResult(response),
                _ => new ObjectResult(response) { StatusCode = status }
            };
        }

        /// <summary>
        /// Retorna uma resposta de solicitação inválida (400) com uma mensagem de erro.
        /// </summary>
        /// <param name="error">Mensagem de erro.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult BadRequest(string error) =>
            CreateResponse(STATUS_400, ERROR, BAD_REQUEST, string.IsNullOrEmpty(error) ? EmptyStringList : new List<string> { error }, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta de solicitação inválida (400) com uma lista de erros.
        /// </summary>
        /// <param name="errors">Lista de erros.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult BadRequest(List<string> errors) =>
            CreateResponse(STATUS_400, ERROR, BAD_REQUEST, errors, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta de solicitação inválida (400) com dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult BadRequest<T>(T data) =>
            CreateResponse(STATUS_400, ERROR, BAD_REQUEST, null, data);

        /// <summary>
        /// Retorna uma resposta de solicitação inválida (400) com uma mensagem de erro e dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="error">Mensagem de erro.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult BadRequest<T>(string error, T? data) =>
            CreateResponse(STATUS_400, ERROR, BAD_REQUEST, string.IsNullOrEmpty(error) ? EmptyStringList : new List<string> { error }, data);

        /// <summary>
        /// Retorna uma resposta de solicitação inválida (400) com uma lista de erros e dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="errors">Lista de erros.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult BadRequest<T>(List<string> errors, T? data) =>
            CreateResponse(STATUS_400, ERROR, BAD_REQUEST, errors, data);

        /// <summary>
        /// Retorna uma resposta de não autorizado (401) com uma mensagem de erro.
        /// </summary>
        /// <param name="error">Mensagem de erro.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Unauthorized(string error) =>
            CreateResponse(STATUS_401, ERROR, UNAUTHORIZED, string.IsNullOrEmpty(error) ? EmptyStringList : new List<string> { error }, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta de não autorizado (401) com uma lista de erros.
        /// </summary>
        /// <param name="errors">Lista de erros.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Unauthorized(List<string> errors) =>
            CreateResponse(STATUS_401, ERROR, UNAUTHORIZED, errors, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta de não autorizado (401) com dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Unauthorized<T>(T data) =>
            CreateResponse(STATUS_401, ERROR, UNAUTHORIZED, null, data);

        /// <summary>
        /// Retorna uma resposta de não autorizado (401) com uma mensagem de erro e dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="error">Mensagem de erro.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Unauthorized<T>(string error, T? data) =>
            CreateResponse(STATUS_401, ERROR, UNAUTHORIZED, string.IsNullOrEmpty(error) ? EmptyStringList : new List<string> { error }, data);

        /// <summary>
        /// Retorna uma resposta de não autorizado (401) com uma lista de erros e dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="errors">Lista de erros.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Unauthorized<T>(List<string> errors, T? data) =>
            CreateResponse(STATUS_401, ERROR, UNAUTHORIZED, errors, data);

        /// <summary>
        /// Retorna uma resposta de sucesso (200) com uma mensagem.
        /// </summary>
        /// <param name="data">Mensagem de sucesso.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Ok(string data) =>
            CreateResponse(STATUS_200, SUCCESS, data, null, NULL_STRING);

        /// <summary>
        /// Retorna uma resposta de sucesso (200) com dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Ok<T>(T data) =>
            CreateResponse(STATUS_200, SUCCESS, null, null, data);

        /// <summary>
        /// Retorna uma resposta de sucesso (200) com uma mensagem e dados.
        /// </summary>
        /// <typeparam name="T">Tipo de dado da resposta.</typeparam>
        /// <param name="message">Mensagem de sucesso.</param>
        /// <param name="data">Dados da resposta.</param>
        /// <returns>Um objeto IActionResult com a resposta HTTP.</returns>
        protected IActionResult Ok<T>(string message, T? data) =>
            CreateResponse(STATUS_200, SUCCESS, message, null, data);
    }

    public class Response<T>
    {
        public int Status { get; }

        public string Title { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; }

        public Response(int status, string title, string? message, List<string>? errors, T? data)
        {
            Status = status;
            Title = title;
            Message = message;
            Errors = errors;
            Data = data;
        }
    }
}
