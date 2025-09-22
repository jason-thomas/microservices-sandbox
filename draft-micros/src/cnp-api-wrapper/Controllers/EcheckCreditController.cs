using System.Threading;
using System.Threading.Tasks;
using cnp_api_wrapper.Handlers;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cnp_api_wrapper.Controllers
{
    [ApiController]
    [Route("api/echeck/credit")]
    public class EcheckCreditController : ControllerBase
    {
        private readonly IEcheckCreditHandler _handler;

        public EcheckCreditController(IEcheckCreditHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<ActionResult<EcheckCreditResponse>> CreateCredit([FromBody] EcheckCreditRequest request, CancellationToken cancellationToken)
        {
            var response = await _handler.HandleAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
