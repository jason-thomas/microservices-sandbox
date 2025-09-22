using System.Threading;
using System.Threading.Tasks;
using cnp_api_wrapper.Handlers;
using cnp_api_wrapper.Models.Requests;
using cnp_api_wrapper.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cnp_api_wrapper.Controllers
{
    [ApiController]
    [Route("api/echeck/debit")]
    public class EcheckDebitController : ControllerBase
    {
        private readonly IEcheckDebitHandler _handler;

        public EcheckDebitController(IEcheckDebitHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<ActionResult<EcheckDebitResponse>> CreateDebit([FromBody] EcheckDebitRequest request, CancellationToken cancellationToken)
        {
            var response = await _handler.HandleAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
