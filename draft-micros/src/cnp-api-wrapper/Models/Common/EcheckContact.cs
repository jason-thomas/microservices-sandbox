using System.ComponentModel.DataAnnotations;

namespace cnp_api_wrapper.Models.Common
{
    public class EcheckContact
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? AddressLine3 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Zip { get; set; }

        public string? Country { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
