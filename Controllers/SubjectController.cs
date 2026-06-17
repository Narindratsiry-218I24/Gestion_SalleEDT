using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Subject")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly SubjectService _service;

        public SubjectController(SubjectService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] Subject subject)
        {
            if (subject == null) return BadRequest();
            var created = await _service.CreateAsync(subject, User?.Identity?.Name);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Subject subject)
        {
            if (subject == null || id != subject.Id) return BadRequest();
            await _service.UpdateAsync(subject, User?.Identity?.Name);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id, User?.Identity?.Name);
            return Ok();
        }
    }
}
