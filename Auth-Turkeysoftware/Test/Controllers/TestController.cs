using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Auth_Turkeysoftware.Test.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : AuthControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserSessionService _loggedUserService;
        private readonly ITestDataRepository _testDataRepository;

        public TestController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService loggedUserService,
            ITestDataRepository testDataRepository) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _loggedUserService = loggedUserService;
            _testDataRepository = testDataRepository;
        }

#if DEBUG
        [HttpPost]
        [Route("teste/{segundos}")]
        public async Task<IActionResult> teste(int segundos)
        {
            Log.Information("Hello, world!");
            Log.Information("Doing magic asynchronously!");
            await _testDataRepository.AddData();
            // Simulate a long running task
            //Thread.Sleep(5000);
            //await Task.Run(() =>
            //{
            //    Log.Information("Doing magic asynchronously!");
            //    // Simulate a long running task
            //    Thread.Sleep(5000);
            //});
            //var email2 = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            //Log.Information();
            return BadRequest(await _testDataRepository.ReadData());
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
                Log.Error($"Houve uma falha na criação de usuário: {result}");
                return BadRequest("Criação de usuário falhou!", result.Errors);
            }

            await CheckAndInsertDefaultRoles();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, nameof(UserRolesEnum.Admin))
            };

            await _userManager.AddToRoleAsync(user, nameof(UserRolesEnum.Admin));
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
#endif
    }
}
