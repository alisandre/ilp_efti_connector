using Microsoft.AspNetCore.Mvc;

namespace ilp_efti_connector.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/transport-operations")]
    public class TransportOperationsController : ControllerBase
    {
        // POST /api/transport-operations
        [HttpPost]
        public IActionResult CreateTransportOperation([FromBody] object payload)
        {
            // TODO: Implement logic to create a transport operation
            // Return 201 Created with location header or appropriate error
            return CreatedAtAction(nameof(GetTransportOperationStatus), new { id = "sample-id" }, new { Id = "sample-id" });
        }

        // GET /api/transport-operations/{id}/status
        [HttpGet("{id}/status")]
        public IActionResult GetTransportOperationStatus(string id)
        {
            // TODO: Implement logic to get the status of a transport operation
            // Return 200 OK with status or 404 NotFound if not found
            return Ok(new { Id = id, Status = "IN_PROGRESS" });
        }
    }
}
