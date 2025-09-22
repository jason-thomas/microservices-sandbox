using System.ComponentModel.DataAnnotations;
using cnp_api_wrapper.Models.Common;

namespace cnp_api_wrapper.Models.Requests
{
    public class EcheckDebitRequest
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Range(0, long.MaxValue)]
        public long Amount { get; set; }

        public string? CustomerId { get; set; }

        public string? ReportGroup { get; set; }

        public string? OrderSource { get; set; }

        [Required]
        public EcheckBankAccount Account { get; set; } = new();

        [Required]
        public EcheckContact Contact { get; set; } = new();
    }
}
