using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.Extensions.Options;

namespace cnp_api_wrapper.Mappings
{
    public class EcheckRedepositMapper : EcheckMappingBase
    {
        public EcheckRedepositMapper(IOptions<CnpOptions> options) : base(options)
        {
        }

        public echeckRedeposit ToSdkRequest(EcheckRedepositRequest request)
        {
            return new echeckRedeposit
            {
                cnpTxnId = request.CnpTransactionId,
                reportGroup = ResolveReportGroup(request.ReportGroup)
            };
        }

        public EcheckRedepositResponse ToApiResponse(echeckRedepositResponse response)
        {
            return new EcheckRedepositResponse
            {
                ResponseCode = response.response,
                Message = response.message,
                CnpTransactionId = response.cnpTxnId,
                ResponseTime = response.responseTime,
                PostDate = response.postDateSpecified ? response.postDate : null,
                Location = response.location
            };
        }
    }
}
