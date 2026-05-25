using APBD_09.DTOs;
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
        
        [HttpPost("{pesel}/bedassignments")]
        public async Task<IActionResult> Post([FromBody] PostBedAssignmentDTO createBedAssignment, [FromRoute] string pesel)
        {
            try
            {
                var bed = _dbService.PostBed(createBedAssignment, pesel);
                return CreatedAtAction("Post", bed);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
