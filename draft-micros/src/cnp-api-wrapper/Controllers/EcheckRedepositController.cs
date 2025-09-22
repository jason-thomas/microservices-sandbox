using System.Threading;
using System.Threading.Tasks;
using cnp_api_wrapper.Handlers;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cnp_api_wrapper.Controllers
{
    [ApiController]
    [Route("api/echeck/redeposit")]
    public class EcheckRedepositController : ControllerBase
    {
        private readonly IEcheckRedepositHandler _handler;

        public EcheckRedepositController(IEcheckRedepositHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<ActionResult<EcheckRedepositResponse>> Redeposit([FromBody] EcheckRedepositRequest request, CancellationToken cancellationToken)
        {
            var response = await _handler.HandleAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
