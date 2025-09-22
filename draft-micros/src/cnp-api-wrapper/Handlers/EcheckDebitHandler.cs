using System.Threading;
using System.Threading.Tasks;
using Cnp.Sdk;
using cnp_api_wrapper.Mappings;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;

namespace cnp_api_wrapper.Handlers
{
    public interface IEcheckDebitHandler
    {
        Task<EcheckDebitResponse> HandleAsync(EcheckDebitRequest request, CancellationToken cancellationToken);
    }

    public class EcheckDebitHandler : IEcheckDebitHandler
    {
        private readonly ICnpOnline _client;
        private readonly EcheckDebitMapper _mapper;

        public EcheckDebitHandler(ICnpOnline client, EcheckDebitMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<EcheckDebitResponse> HandleAsync(EcheckDebitRequest request, CancellationToken cancellationToken)
        {
            var sdkRequest = _mapper.ToSdkRequest(request);
            var response = await _client.EcheckSaleAsync(sdkRequest, cancellationToken).ConfigureAwait(false);
            return _mapper.ToApiResponse(response);
        }
    }
}
