using System;

namespace cnp_api_wrapper.Models.Responses
{
    public class EcheckVoidResponse
    {
        public string ResponseCode { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public long CnpTransactionId { get; set; }

        public DateTime ResponseTime { get; set; }

        public DateTime? PostDate { get; set; }

        public string? Location { get; set; }
    }
}
