using System.Threading;
using System.Threading.Tasks;
using Cnp.Sdk;
using cnp_api_wrapper.Mappings;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;

namespace cnp_api_wrapper.Handlers
{
    public interface IEcheckVoidHandler
    {
        Task<EcheckVoidResponse> HandleAsync(EcheckVoidRequest request, CancellationToken cancellationToken);
    }

    public class EcheckVoidHandler : IEcheckVoidHandler
    {
        private readonly ICnpOnline _client;
        private readonly EcheckVoidMapper _mapper;

        public EcheckVoidHandler(ICnpOnline client, EcheckVoidMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<EcheckVoidResponse> HandleAsync(EcheckVoidRequest request, CancellationToken cancellationToken)
        {
            var sdkRequest = _mapper.ToSdkRequest(request);
            var response = await _client.EcheckVoidAsync(sdkRequest, cancellationToken).ConfigureAwait(false);
            return _mapper.ToApiResponse(response);
        }
    }
}
