using APBD_09.Exceptions;
using APBD_09.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD_09.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IDBService _dbService;
        public PatientsController(IDBService dbService)
        {
            _dbService = dbService; 
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromRoute]string? search)
        {
            try
            {
                var patients = _dbService.GetPatients(search);
                return Ok(patients);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
