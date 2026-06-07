using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApiProject.Data.Entities;
using TodoApiProject.Models.RequestModels;
using TodoApiProject.Models.RequestModels.Todo;
using TodoApiProject.Services.Interfaces;

namespace TodoApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTodoRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var todo = await _todoService.CreateTodoAsync(request, userId);

            return Ok(todo);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] TodoListFilterRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var todos = await _todoService.GetAllTodosAsync(request, userId);

            return Ok(todos);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _todoService.GetTodoByIdAsync(id, userId);

            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTodoRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _todoService.UpdateTodoAsync(request, id, userId);

            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _todoService.DeleteTodoAsync(id, userId);

            return Ok(response);
        }

        [HttpPatch("completed/{id}")]
        public async Task<IActionResult> CompleteAsync(Guid id, [FromQuery] bool isCompleted)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _todoService.CompleteTodoAsync(id, isCompleted, userId);

            return Ok(response);
        }
    }
}
