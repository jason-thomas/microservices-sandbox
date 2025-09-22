using System.Threading;
using System.Threading.Tasks;
using Cnp.Sdk;
using cnp_api_wrapper.Mappings;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;

namespace cnp_api_wrapper.Handlers
{
    public interface IEcheckRedepositHandler
    {
        Task<EcheckRedepositResponse> HandleAsync(EcheckRedepositRequest request, CancellationToken cancellationToken);
    }

    public class EcheckRedepositHandler : IEcheckRedepositHandler
    {
        private readonly ICnpOnline _client;
        private readonly EcheckRedepositMapper _mapper;

        public EcheckRedepositHandler(ICnpOnline client, EcheckRedepositMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<EcheckRedepositResponse> HandleAsync(EcheckRedepositRequest request, CancellationToken cancellationToken)
        {
            var sdkRequest = _mapper.ToSdkRequest(request);
            var response = await _client.EcheckRedepositAsync(sdkRequest, cancellationToken).ConfigureAwait(false);
            return _mapper.ToApiResponse(response);
        }
    }
}
