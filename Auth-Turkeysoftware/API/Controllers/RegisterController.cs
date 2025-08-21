using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Auth_Turkeysoftware.API.Bases;
using Auth_Turkeysoftware.API.Models.Request;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Shared.Exceptions;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;

namespace Auth_Turkeysoftware.API.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class RegisterController : CommonControllerBase
    {
        private readonly IRegisterUserService _registerUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterController> _logger;

        private const string ERRO_CADASTRO = "Houve um erro durante a criação do usuário. Tente novamente mais tarde.";
        public RegisterController(IRegisterUserService registerUserService, UserManager<ApplicationUser> userManager, ILogger<RegisterController> logger)
        {
            _registerUserService = registerUserService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo usuário no sistema com perfil padrão.
        /// </summary>
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

            if (!registerUserResult.IsSuccess())
            {
                if (registerUserResult.UserAlreadyExists)
                {
                    return BadRequest("Usuário já existe!");
                }
                else if (registerUserResult.IdentityErrorList != null && registerUserResult.IdentityErrorList.Any())
                {
                    foreach (IdentityError errors in registerUserResult.IdentityErrorList)
                    {
                        ModelState.AddModelError(errors.Code, errors.Description);
                    }
                    return BadRequest(ERRO_CADASTRO);
                }
                else
                {
                    ModelState.AddModelError("UnknownError", registerUserResult.ExceptionMessage);
                    return BadRequest(ERRO_CADASTRO);
                }
            }

            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null)
                {
                    throw new BusinessException("Não foi possível encontrar o cadastro do usuário.");
                }
                await _registerUserService.SendConfirmEmailRequest(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível enviar o email de confirmação.");
            }

            return Ok("Usuário cadastrado com sucesso!");
        }

        /// <summary>
        /// Confirma o endereço de email de um usuário utilizando o token de confirmação.
        /// </summary>
        /// <param name="request">Dados para confirmação do email contendo:
        /// <list type="bullet">
        /// <item><description>userId: ID do usuário</description></item>
        /// <item><description>email: Endereço de email a ser confirmado</description></item>
        /// <item><description>token: Token de confirmação recebido por email</description></item>
        /// </list>
        /// </param>
        /// <returns>Resultado da operação de confirmação de email.</returns>
        /// <response code="200">Mail confirmado com sucesso.</response>
        /// <response code="400">
        /// Falha na confirmação devido a:
        /// <list type="bullet">
        /// <item><description>Usuário não encontrado</description></item>
        /// <item><description>Mail não corresponde ao usuário</description></item>
        /// <item><description>Token inválido ou expirado</description></item>
        /// </list>
        /// </response>
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new BusinessException("Usuário não encontrado");
            }
            else if (user.Email != request.Email)
            {
                throw new BusinessException("Usuário não encontrado");
            }

            var result = await _registerUserService.ConfirmEmailRequest(user, request.token);
            if (!result)
            {
                return BadRequest("Não foi possível confirmar o email do usuário.");
            }

            return Ok();
        }
    }
}
