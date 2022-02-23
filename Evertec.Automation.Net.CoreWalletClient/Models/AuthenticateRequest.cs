using System.ComponentModel.DataAnnotations;

namespace Evertec.Automation.Net.CoreWalletClient.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
