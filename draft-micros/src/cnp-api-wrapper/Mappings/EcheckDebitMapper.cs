using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.Extensions.Options;

namespace cnp_api_wrapper.Mappings
{
    public class EcheckDebitMapper : EcheckMappingBase
    {
        public EcheckDebitMapper(IOptions<CnpOptions> options) : base(options)
        {
        }

        public echeckSale ToSdkRequest(EcheckDebitRequest request)
        {
            var sdkRequest = new echeckSale
            {
                orderId = request.OrderId,
                amount = request.Amount,
                customerId = request.CustomerId,
                reportGroup = ResolveReportGroup(request.ReportGroup),
                orderSource = ResolveOrderSource(request.OrderSource),
                billToAddress = MapContact(request.Contact),
                echeck = MapAccount(request.Account)
            };

            return sdkRequest;
        }

        public EcheckDebitResponse ToApiResponse(echeckSalesResponse response)
        {
            return new EcheckDebitResponse
            {
                ResponseCode = response.response,
                Message = response.message,
                CnpTransactionId = response.cnpTxnId,
                ResponseTime = response.responseTime,
                PostDate = response.postDateSpecified ? response.postDate : null,
                Location = response.location,
                VerificationCode = response.verificationCode
            };
        }
    }
}
