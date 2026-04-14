using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _unitOfWork.Repository<AuditLog>().GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _unitOfWork.Repository<AuditLog>().GetByIdAsync(id);
            if (log == null) return NotFound();
            return Ok(log);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AuditLog log)
        {
            await _unitOfWork.Repository<AuditLog>().AddAsync(log);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _unitOfWork.Repository<AuditLog>().GetByIdAsync(id);
            if (log == null) return NotFound();
            _unitOfWork.Repository<AuditLog>().Delete(log);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
    }
}