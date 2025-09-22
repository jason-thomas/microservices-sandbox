using System.Threading;
using System.Threading.Tasks;
using cnp_api_wrapper.Handlers;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cnp_api_wrapper.Controllers
{
    [ApiController]
    [Route("api/echeck/void")]
    public class EcheckVoidController : ControllerBase
    {
        private readonly IEcheckVoidHandler _handler;

        public EcheckVoidController(IEcheckVoidHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<ActionResult<EcheckVoidResponse>> Void([FromBody] EcheckVoidRequest request, CancellationToken cancellationToken)
        {
            var response = await _handler.HandleAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
