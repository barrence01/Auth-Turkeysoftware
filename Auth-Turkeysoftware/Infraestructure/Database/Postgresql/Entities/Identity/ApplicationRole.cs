using Microsoft.AspNetCore.Identity;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
