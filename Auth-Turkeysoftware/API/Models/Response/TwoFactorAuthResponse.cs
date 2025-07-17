using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.API.Models.Response
{
    public class TwoFactorAuthResponse
    {
        public int? TwoFactorMode { get; set; }
        public string? To { get; set; }
    }
}
