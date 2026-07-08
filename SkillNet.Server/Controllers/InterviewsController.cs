using Microsoft.AspNetCore.Mvc;
using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;

namespace SkillNet.Server.Controllers
{
    [Route("api/interviews")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewService _service;

        public InterviewsController(IInterviewService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var interviews = await _service.GetAllAsync();
            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var interview = await _service.GetByIdAsync(id);

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Interview interview)
        {
            var createdInterview = await _service.CreateAsync(interview);
            return CreatedAtAction(nameof(GetById), new { id = createdInterview.InterviewId }, createdInterview);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Interview interview)
        {
            if (id != interview.InterviewId)
                return BadRequest();

            var updatedInterview = await _service.UpdateAsync(interview);

            if (updatedInterview == null)
                return NotFound();

            return Ok(updatedInterview);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}