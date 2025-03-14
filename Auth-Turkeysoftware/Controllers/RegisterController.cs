using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class RegisterController : CommonControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterController (
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);

            if (userExists != null)
            {
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

            if (!result.Succeeded)
            {
                Log.Error($"Houve uma falha na criação de usuário: {result}");
                return BadRequest("Criação de usuário falhou!", result.Errors);
            }
            await CheckAndInsertDefaultRoles();

            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, UserRolesEnum.User.ToString())
            };

            await _userManager.AddToRoleAsync(user, UserRolesEnum.User.ToString());
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
