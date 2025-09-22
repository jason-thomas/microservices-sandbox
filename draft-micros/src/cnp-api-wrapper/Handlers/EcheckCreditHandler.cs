using System.Threading;
using System.Threading.Tasks;
using Cnp.Sdk;
using cnp_api_wrapper.Mappings;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;

namespace cnp_api_wrapper.Handlers
{
    public interface IEcheckCreditHandler
    {
        Task<EcheckCreditResponse> HandleAsync(EcheckCreditRequest request, CancellationToken cancellationToken);
    }

    public class EcheckCreditHandler : IEcheckCreditHandler
    {
        private readonly ICnpOnline _client;
        private readonly EcheckCreditMapper _mapper;

        public EcheckCreditHandler(ICnpOnline client, EcheckCreditMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<EcheckCreditResponse> HandleAsync(EcheckCreditRequest request, CancellationToken cancellationToken)
        {
            var sdkRequest = _mapper.ToSdkRequest(request);
            var response = await _client.EcheckCreditAsync(sdkRequest, cancellationToken).ConfigureAwait(false);
            return _mapper.ToApiResponse(response);
        }
    }
}
