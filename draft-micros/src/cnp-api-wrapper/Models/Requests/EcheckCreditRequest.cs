using System.ComponentModel.DataAnnotations;
using cnp_api_wrapper.Models.Common;

namespace cnp_api_wrapper.Models.Requests
{
    public class EcheckCreditRequest
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Range(0, long.MaxValue)]
        public long Amount { get; set; }

        public string? CustomerId { get; set; }

        public string? ReportGroup { get; set; }

        public string? OrderSource { get; set; }

        public long? CnpTransactionId { get; set; }

        public EcheckBankAccount? Account { get; set; }

        public EcheckContact? Contact { get; set; }
    }
}
