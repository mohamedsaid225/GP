using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuspiciousEventsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public SuspiciousEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _unitOfWork.Repository<SuspiciousEvent>().GetAllAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _unitOfWork.Repository<SuspiciousEvent>().GetByIdAsync(id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SuspiciousEvent ev)
        {
            await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(ev);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SuspiciousEvent ev)
        {
            if (id != ev.Id) return BadRequest();
            _unitOfWork.Repository<SuspiciousEvent>().Update(ev);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _unitOfWork.Repository<SuspiciousEvent>().GetByIdAsync(id);
            if (ev == null) return NotFound();
            _unitOfWork.Repository<SuspiciousEvent>().Delete(ev);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
    }
}