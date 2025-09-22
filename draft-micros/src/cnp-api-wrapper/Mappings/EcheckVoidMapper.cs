using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.Extensions.Options;

namespace cnp_api_wrapper.Mappings
{
    public class EcheckVoidMapper : EcheckMappingBase
    {
        public EcheckVoidMapper(IOptions<CnpOptions> options) : base(options)
        {
        }

        public echeckVoid ToSdkRequest(EcheckVoidRequest request)
        {
            return new echeckVoid
            {
                cnpTxnId = request.CnpTransactionId,
                reportGroup = ResolveReportGroup(request.ReportGroup)
            };
        }

        public EcheckVoidResponse ToApiResponse(echeckVoidResponse response)
        {
            return new EcheckVoidResponse
            {
                ResponseCode = response.response,
                Message = response.message,
                CnpTransactionId = response.cnpTxnId,
                ResponseTime = response.responseTime,
                PostDate = response.postDate == default ? null : response.postDate,
                Location = response.location
            };
        }
    }
}
