using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    public class TwoFactorAuthResponse
    {
        public int? TwoFactorMode { get; set; }
        public string? To {  get; set; }
    }
}
