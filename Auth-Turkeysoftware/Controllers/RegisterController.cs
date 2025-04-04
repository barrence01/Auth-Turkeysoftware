using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Services;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : CommonControllerBase
    {
        private readonly IRegisterUserService _registerUserService;

        private const string ERRO_CADASTRO = "Houve um erro durante a criação do usuário. Tente novamente mais tarde.";
        public RegisterController (
            IRegisterUserService registerUserService)
        {
            _registerUserService = registerUserService;
        }

        /// <summary>
        /// Registra um novo usuário no sistema com perfil padrão.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Register/register-user<br/>
        ///     {
        ///         "email": "novo.usuario@exemplo.com",
        ///         "name": "Fulano da Silva",
        ///         "phoneNumber": "11999999999",
        ///         "password": "SenhaForte@123"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados para cadastro do novo usuário.</param>
        /// <returns>Resultado da operação de cadastro.</returns>
        /// <response code="200">Usuário registrado com sucesso.</response>
        /// <response code="400">Falha no registro (usuário já existe ou dados inválidos).</response>
        [HttpPost("register-user")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {

            var registerUserResult = await _registerUserService.RegisterUser(request);

            if (!registerUserResult.HasSucceeded()) {
                if (registerUserResult.UserAlreadyExists) {
                    return BadRequest("Usuário já existe!");
                }
                else if (registerUserResult.identityErrors != null && registerUserResult.identityErrors.Any()) {
                    foreach(IdentityError errors in registerUserResult.identityErrors) {
                        ModelState.AddModelError(errors.Code, errors.Description);
                    }
                    return BadRequest(ERRO_CADASTRO);
                }
                else {
                    ModelState.AddModelError("UnknownError", registerUserResult.ExceptionMessage);
                    return BadRequest(ERRO_CADASTRO);
                }
            }

            return Ok("Usuário cadastrado com sucesso!");
        }
    }
}
