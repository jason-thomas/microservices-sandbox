using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cnp_api_wrapper.Configuration
{
    public class CnpOptions
    {
        private const string DefaultClientId = "cnp-api-wrapper";

        [Required]
        public string MerchantId { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = "https://www.testvantivcnp.com/sandbox/communicator/online";

        [Required]
        public string ReportGroup { get; set; } = "Default Report Group";

        public string? Id { get; set; }

        public int TimeoutMilliseconds { get; set; } = 35000;

        public bool PrintXml { get; set; }

        public string? ProxyHost { get; set; }

        public int? ProxyPort { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var config = new Dictionary<string, string>
            {
                ["merchantId"] = MerchantId,
                ["username"] = Username,
                ["password"] = Password,
                ["url"] = Url,
                ["reportGroup"] = ReportGroup,
                ["printxml"] = PrintXml.ToString().ToLowerInvariant(),
                ["timeout"] = TimeoutMilliseconds.ToString(),
                ["id"] = string.IsNullOrWhiteSpace(Id) ? DefaultClientId : Id
            };

            if (!string.IsNullOrWhiteSpace(ProxyHost))
            {
                config["proxyHost"] = ProxyHost;
            }

            if (ProxyPort.HasValue)
            {
                config["proxyPort"] = ProxyPort.Value.ToString();
            }

            return config;
        }
    }
}
