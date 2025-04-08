using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Helpers;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Result;
using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Services
{
    public class RegisterUserService : IRegisterUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterUserService> _logger;
        internal AppDbContext _dbContext;

        public RegisterUserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterUserService> logger,
            AppDbContext dataBaseContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _dbContext = dataBaseContext;
        }

        public async Task<RegisterUserResult> RegisterUser(RegisterRequest model)
        {
            RegisterUserResult result = new RegisterUserResult();

            try
            {
                return await TransactionHelper.ExecuteWithTransactionAsync(_dbContext, async () =>
                {

                    var userExists = await _userManager.FindByNameAsync(model.Email);

                    if (userExists != null) {
                        result.UserAlreadyExists = true;
                        return result;
                    }

                    ApplicationUser user = new()
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber
                    };

                    var createAsyncresult = await _userManager.CreateAsync(user, model.Password);

                    if (!createAsyncresult.Succeeded) {
                        _logger.LogError("Houve uma falha durante a criação do usuário: {CreateAsyncresult}", createAsyncresult);
                        result.identityErrors = createAsyncresult.Errors;
                        throw new BusinessException("Houve uma falha durante a criação do usuário");
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

                    return result;
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Erro durante o registro do usuário");
                result.HasExceptionThrow = true;
                result.ExceptionMessage = ex.Message;

                return result;
            }
        }

        private async Task CheckAndInsertDefaultRoles()
        {
            var roles = _roleManager.Roles.ToList();

            foreach (string userRole in Enum.GetNames(typeof(UserRolesEnum)))
            {
                if (!roles.Any(r => r.Name == userRole)) {
                    await _roleManager.CreateAsync(new IdentityRole(userRole));
                }
            }
        }
    }
}
