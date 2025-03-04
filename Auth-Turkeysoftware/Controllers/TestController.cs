using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILoggedUserService _loggedUserService;

        public TestController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILoggedUserService loggedUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _loggedUserService = loggedUserService;
        }

        [HttpPost]
        [Route("teste/{segundos}")]
        public async Task<IActionResult> teste(int segundos)
        {
            Log.Information("Hello, world!");
            Log.Information("Doing magic asynchronously!");
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
            return Ok();
        }
    }
}
