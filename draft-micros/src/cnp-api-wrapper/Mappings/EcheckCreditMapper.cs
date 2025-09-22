using System;
using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.Extensions.Options;

namespace cnp_api_wrapper.Mappings
{
    public class EcheckCreditMapper : EcheckMappingBase
    {
        public EcheckCreditMapper(IOptions<CnpOptions> options) : base(options)
        {
        }

        public echeckCredit ToSdkRequest(EcheckCreditRequest request)
        {
            var sdkRequest = new echeckCredit
            {
                orderId = request.OrderId,
                amount = request.Amount,
                customerId = request.CustomerId,
                reportGroup = ResolveReportGroup(request.ReportGroup),
                orderSource = ResolveOrderSource(request.OrderSource)
            };

            if (request.CnpTransactionId.HasValue)
            {
                sdkRequest.cnpTxnId = request.CnpTransactionId.Value;
            }
            else
            {
                if (request.Account is null)
                {
                    throw new ArgumentException("Account information is required when cnpTransactionId is not provided.", nameof(request));
                }

                sdkRequest.echeck = MapAccount(request.Account);
                sdkRequest.billToAddress = MapContact(request.Contact);
            }

            return sdkRequest;
        }

        public EcheckCreditResponse ToApiResponse(echeckCreditResponse response)
        {
            return new EcheckCreditResponse
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
