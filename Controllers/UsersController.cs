using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers() =>
            await _context.Usuarios.Find(_ => true).ToListAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user is null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<IActionResult> CriarUser(User novoUser)
        {
            await _context.Usuarios.InsertOneAsync(novoUser);
            return CreatedAtAction(nameof(GetUser), new { id = novoUser.Id }, novoUser);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> AtualizarUser(string id, User userAtualizado)
        {
            var user = await _context.Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user is null)
            {
                return NotFound();
            }

            userAtualizado.Id = user.Id;
            await _context.Usuarios.ReplaceOneAsync(u => u.Id == id, userAtualizado);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeletarUser(string id)
        {
            var user = await _context.Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user is null)
            {
                return NotFound();
            }

            await _context.Usuarios.DeleteOneAsync(u => u.Id == id);

            return NoContent();
        }
    }
}