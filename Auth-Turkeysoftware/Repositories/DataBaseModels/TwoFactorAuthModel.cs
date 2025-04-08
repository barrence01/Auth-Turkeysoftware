using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core.Tokens;
using Microsoft.EntityFrameworkCore;
using Auth_Turkeysoftware.Extensions;

namespace Auth_Turkeysoftware.Repositories.DataBaseModels
{
    [Table("tb_two_factor_auth", Schema = "auth")]
    [Index(nameof(FkUserId), Name = "IX_fk_id_usuar_2fa")]
    public class TwoFactorAuthModel
    {
        [Key]
        [Column("id_two_factor")]
        public int? TwoFactorId { get; set; }

        [Column("fk_id_usuario")]
        [Required]
        public string? FkUserId { get; set; }

        [Column("in_two_factor_mode")]
        [Required]
        [AllowedValues(1, 2, 3)]
        [Comment("1 - Email | 2 - SMS | 3 - Whatsapp | 4 - TOTP")]
        public int? TwoFactorMode { get; set; }

        [Column("tx_required_params", TypeName = "jsonb")]
        [Comment("Um JSON contendo as informações necessárias para validar este tipo de autenticação")]
        public byte[]? RequiredParameters { get; set; }

        [Column("in_reg_ativo")]
        [Required]
        [Comment("true - registro ativo | false - registro inativo")]
        public bool? IsActive { get; set; }

        [Column("dt_inclusao")]
        [Required]
        public DateTime? CreatedOn { get; set; }

        [Column("dt_alteracao")]
        public DateTime? UpdatedOn { get; set; }

        [Required]
        public ApplicationUser? User { get; set; }

        public TwoFactorAuthModel() { }

        public TwoFactorAuthModel(string userId, int twoFactorMode) {
            FkUserId = userId;
            TwoFactorMode = twoFactorMode;
        }

        public bool IsValidForInclusion()
        {
            if (FkUserId == null || TwoFactorMode == null || TwoFactorMode <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
