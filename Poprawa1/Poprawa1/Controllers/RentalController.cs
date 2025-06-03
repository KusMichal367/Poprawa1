using Poprawa1.Services;
using Poprawa1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Poprawa1.Controllers;

[Route("api/[controller]")]
[ApiController]

public class ClientsController : ControllerBase
{
    private readonly IRentalService _iRentalService;

    public ClientsController(IRentalService iRentalService)
    {
        _iRentalService = iRentalService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        try
        {
            var client = await _iRentalService.GetClientAsync(id);
            return Ok(client);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (SqlException ex)
        {
            return StatusCode(500, ex.Message);
        }

    }


    [HttpPost]
    public async Task<IActionResult> AddClientAsync([FromBody]InputDTO rental)
    {

        try
        {
            await _iRentalService.AddClientAndRentalAsync(rental);

        }
        catch (ArgumentException argEx)
        {
            return NotFound(argEx.Message);
        }

        catch (InvalidOperationException ioEx)
        {
            return Conflict(ioEx.Message);
        }

        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return StatusCode(201, rental);
    }

}