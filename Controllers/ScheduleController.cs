using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly SchedulingService _service;
        private readonly EMITDbContext _context;

        public ScheduleController(SchedulingService service, EMITDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetAll()
        {
            var list = _context.Schedules.ToList();
            return Ok(list);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] Schedule schedule)
        {
            if (schedule == null) return BadRequest();
            try
            {
                var created = await _service.CreateScheduleAsync(schedule, User?.Identity?.Name);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
