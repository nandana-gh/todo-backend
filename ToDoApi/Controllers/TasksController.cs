using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;

namespace ToDoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID is missing from token.");
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try 
            {
                var userId = GetUserId();
                var tasks = await _context.Tasks
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (UnauthorizedAccessException) 
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDto model)
        {
            try
            {
                var userId = GetUserId();

                var task = new TodoTask
                {
                    UserId = userId,
                    Title = model.Title
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
            }
            catch (UnauthorizedAccessException) 
            {
                return Unauthorized();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto model)
        {
            try 
            {
                var userId = GetUserId();
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                task.IsCompleted = model.IsCompleted;
                await _context.SaveChangesAsync();

                return Ok(task);
            }
            catch (UnauthorizedAccessException) 
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            try 
            {
                var userId = GetUserId();
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Task deleted successfully" });
            }
            catch (UnauthorizedAccessException) 
            {
                return Unauthorized();
            }
        }
    }
}
