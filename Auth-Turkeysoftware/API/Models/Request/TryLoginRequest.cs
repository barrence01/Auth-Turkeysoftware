﻿using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.API.Models.Request
{
    public class TryLoginRequest
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        public string Email
        {
            get => _email;
            set => _email = value.ToLower();
        }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; } = string.Empty;

    }
}
