using System.ComponentModel.DataAnnotations;

namespace cnp_api_wrapper.Models.Requests
{
    public class EcheckVoidRequest
    {
        [Required]
        public long CnpTransactionId { get; set; }

        public string? ReportGroup { get; set; }
    }
}
