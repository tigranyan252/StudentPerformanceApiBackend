// Path: StudentPerformance.Api/Controllers/DebugController.cs
// Обновите этот файл в папке Controllers вашего проекта StudentPerformance.Api

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPerformance.Api.Data; // Убедитесь, что это правильный namespace для вашего DbContext
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using StudentPerformance.Api.Data.Entities; // Для доступа к вашим моделям сущностей (User, Group и т.д.)

namespace StudentPerformance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Закомментируйте или удалите, чтобы можно было получить доступ без токена
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the list of applied Entity Framework Core migrations.
        /// This is for debugging purposes and should be removed in production.
        /// </summary>
        /// <returns>A list of applied migration IDs.</returns>
        [HttpGet("migrations")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> GetAppliedMigrations()
        {
            try
            {
                var migrations = await _context.Database.GetAppliedMigrationsAsync();
                return Ok(migrations.ToList());
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// This is for debugging purposes ONLY and must be removed in production.
        /// </summary>
        /// <returns>A list of User entities.</returns>
        [HttpGet("users-raw")]
        [AllowAnonymous] // Разрешаем доступ без аутентификации для удобства отладки
        public async Task<ActionResult<IEnumerable<User>>> GetRawUsers()
        {
            try
            {
                // Возвращаем все записи из таблицы Users
                // ВНИМАНИЕ: Это возвращает хешированные пароли. Для реального API
                // нужно использовать DTOs, чтобы не раскрывать чувствительные данные.
                return Ok(await _context.Users.ToListAsync());
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred fetching raw users: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all groups from the database.
        /// This is for debugging purposes ONLY and must be removed in production.
        /// </summary>
        /// <returns>A list of Group entities.</returns>
        [HttpGet("groups-raw")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Group>>> GetRawGroups()
        {
            try
            {
                return Ok(await _context.Groups.ToListAsync());
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred fetching raw groups: {ex.Message}");
            }
        }

        // Вы можете добавлять аналогичные методы для других таблиц, например:
        // [HttpGet("students-raw")]
        // [AllowAnonymous]
        // public async Task<ActionResult<IEnumerable<Student>>> GetRawStudents()
        // {
        //     try
        //     {
        //         return Ok(await _context.Students.ToListAsync());
        //     }
        //     catch (System.Exception ex)
        //     {
        //         return StatusCode(500, $"An error occurred fetching raw students: {ex.Message}");
        //     }
        // }
    }
}
