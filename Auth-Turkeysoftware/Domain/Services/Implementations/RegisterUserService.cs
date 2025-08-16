using Auth_Turkeysoftware.API.Models.Request;
using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Shared.Exceptions;
using Auth_Turkeysoftware.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class RegisterUserService : IRegisterUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICommunicationService _commService;
        private readonly ILogger<RegisterUserService> _logger;
        internal AppDbContext _dbContext;

        public RegisterUserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICommunicationService commService,
            ILogger<RegisterUserService> logger,
            AppDbContext dataBaseContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _commService = commService;
            _logger = logger;
            _dbContext = dataBaseContext;
        }

        public async Task<RegisterUserResult> RegisterUser(RegisterRequest request)
        {
            RegisterUserResult result = new();

            try
            {
                return await TransactionHelper.ExecuteWithTransactionAsync(_dbContext, async () =>
                {

                    var userExists = await _userManager.FindByNameAsync(request.Email);

                    if (userExists != null)
                    {
                        result.UserAlreadyExists = true;
                        return result;
                    }

                    ApplicationUser user = new()
                    {
                        Email = request.Email,
                        UserName = request.Email,
                        Name = request.Name,
                        PhoneNumber = request.PhoneNumber
                    };

                    var createUserResult = await _userManager.CreateAsync(user, request.Password);

                    if (!createUserResult.Succeeded)
                    {
                        _logger.LogError("Houve uma falha durante a criação do usuário: {CreateAsyncresult}", createUserResult);
                        result.IdentityErrorList = createUserResult.Errors;
                        throw new BusinessException("Houve uma falha durante a criação do usuário");
                    }

                    await CheckAndInsertDefaultRoles();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, nameof(UserRolesEnum.User))
                    };

                    await _userManager.AddToRoleAsync(user, nameof(UserRolesEnum.User));
                    await _userManager.AddClaimsAsync(user, claims);

                    return result;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o registro do usuário");
                result.HasException = true;
                result.ExceptionMessage = ex.Message;

                return result;
            }
        }

        public async Task SendConfirmEmailRequest(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _commService.SendConfirmEmailRequest(user.Id, user.Email!, token);
        }

        public async Task<bool> ConfirmEmailRequest(ApplicationUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
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
    }
}
