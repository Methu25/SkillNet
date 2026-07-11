using Microsoft.AspNetCore.Mvc;
using SkillNet.Server.DTOs;
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
            var interviews = await _service.GetAllInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var interview = await _service.GetInterviewByIdAsync(id);

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInterviewRequest request)
        {
            var createdInterview = await _service.CreateInterviewAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdInterview.InterviewId }, createdInterview);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateInterviewRequest request)
        {
            var updatedInterview = await _service.UpdateInterviewAsync(id, request);

            if (updatedInterview == null)
                return NotFound();

            return Ok(updatedInterview);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteInterviewAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/schedule")]
        public async Task<IActionResult> ScheduleInterview(int id, ScheduleInterviewRequest request)
        {
            var interview = await _service.ScheduleInterviewAsync(id, request);

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> RescheduleInterview(int id, ScheduleInterviewRequest request)
        {
            var interview = await _service.RescheduleInterviewAsync(id, request);

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelInterview(int id)
        {
            var interview = await _service.CancelInterviewAsync(id);

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPost("{id}/evaluation")]
        public async Task<IActionResult> CreateEvaluation(int id, CreateEvaluationRequest request)
        {
            var evaluation = await _service.CreateEvaluationAsync(id, request);
            return Ok(evaluation);
        }

        [HttpGet("{id}/evaluation")]
        public async Task<IActionResult> GetEvaluation(int id)
        {
            var evaluation = await _service.GetEvaluationByInterviewIdAsync(id);

            if (evaluation == null)
                return NotFound();

            return Ok(evaluation);
        }

        [HttpPut("{id}/evaluation")]
        public async Task<IActionResult> UpdateEvaluation(int id, CreateEvaluationRequest request)
        {
            var evaluation = await _service.UpdateEvaluationAsync(id, request);

            if (evaluation == null)
                return NotFound();

            return Ok(evaluation);
        }
    }
}