using Auth_Turkeysoftware.API.Bases;
using Auth_Turkeysoftware.API.Models.Request;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Configurations.Singletons;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Test.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace Auth_Turkeysoftware.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : AuthControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public TestController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService loggedUserService,
            ITestDataRepository testDataRepository) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

#if DEBUG
        [HttpPost]
        [Route("teste/{segundos}")]
        public async Task<IActionResult> teste(int segundos)
        {
            Log.Information("Hello, world!");
            Log.Information("Doing magic asynchronously!");
            // Simulate a long running task
            await Task.Run(() =>
            {
                Log.Information("Doing magic asynchronously!");
                // Simulate a long running task
                Thread.Sleep(5000);
            });
            return BadRequest();
        }

        [HttpPost]
        [Route("teste/create-default-user")]
        public async Task<IActionResult> CreateDefaultUser()
        {
            RegisterRequest? model = null;
            string email = "desenv@email.com";

            model = new RegisterRequest()
            {
                Name = "desenv",
                Email = email,
                Password = "Pass123@",
                PhoneNumber = "00000000"
            };

            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                await RegisterMaster(model);
            }
            else
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, resetToken, model.Password);
            }

            return Ok("Usuario Criado com Sucesso", model);
        }

        [HttpPost]
        [Route("admin/register-master")]
        public async Task<IActionResult> RegisterMaster([FromBody] RegisterRequest model)
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
                Log.Error("Houve uma falha na criação de usuário: {Result}", result);
                return BadRequest("Criação de usuário falhou!", result.Errors);
            }

            await CheckAndInsertDefaultRoles();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, nameof(UserRolesEnum.Admin))
            };

            await _userManager.AddToRoleAsync(user, nameof(UserRolesEnum.Admin));
            await _userManager.AddClaimsAsync(user, claims);

            return Ok("Usuário criado com sucesso!");
        }

        private async Task CheckAndInsertDefaultRoles()
        {
            var roles = _roleManager.Roles.ToList();

            foreach (string userRole in Enum.GetNames(typeof(UserRolesEnum)))
            {
                if (!roles.Any(r => r.Name == userRole))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(userRole));
                }
            }
        }
#endif
    }
}
