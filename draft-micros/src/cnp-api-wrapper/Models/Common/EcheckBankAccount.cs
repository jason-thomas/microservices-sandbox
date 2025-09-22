using System.ComponentModel.DataAnnotations;

namespace cnp_api_wrapper.Models.Common
{
    public class EcheckBankAccount
    {
        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        public string RoutingNumber { get; set; } = string.Empty;

        [Required]
        public string AccountType { get; set; } = string.Empty;

        public string? CheckNumber { get; set; }

        public string? AccountHolderName { get; set; }
    }
}
