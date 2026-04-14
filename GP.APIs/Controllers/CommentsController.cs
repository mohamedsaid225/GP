using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // أي حد يضيف comment
        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            var comment = new Comment
            {
                Name = dto.Name,
                Email = dto.Email,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Comment added successfully", comment.Id });
        }

        // الدكتور يشوف كل الـ Comments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _unitOfWork.Repository<Comment>().GetAllAsync();
            return Ok(comments);
        }

        // مسح Comment
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return NotFound();

            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
