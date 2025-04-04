using Auth_Turkeysoftware.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Authorization;
using Auth_Turkeysoftware.Models.Response;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : CommonControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController (
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
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
        /// <param name="model">Dados para cadastro do novo usuário.</param>
        /// <returns>Resultado da operação de cadastro.</returns>
        /// <response code="200">Usuário registrado com sucesso.</response>
        /// <response code="400">Falha no registro (usuário já existe ou dados inválidos).</response>
        [HttpPost("register-user")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);

            if (userExists != null) {
                return BadRequest("Usuário já existe!");
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) {
                _logger.LogError($"Houve uma falha na criação de usuário: {result}");
                return BadRequest("Criação de usuário falhou!", result.Errors);
            }
            await CheckAndInsertDefaultRoles();

            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, nameof(UserRolesEnum.User))
            };

            await _userManager.AddToRoleAsync(user, nameof(UserRolesEnum.User));
            await _userManager.AddClaimsAsync(user, claims);

            return Ok("Usuário criado com sucesso!");
        }

        private async Task<bool> CheckAndInsertDefaultRoles()
        {
            foreach (string userRole in Enum.GetNames(typeof(UserRolesEnum)))
            {
                if (!await _roleManager.RoleExistsAsync(userRole.ToString()))
                    await _roleManager.CreateAsync(new IdentityRole(userRole.ToString()));
            }
            return true;
        }
    }
}
