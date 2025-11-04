using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ControleEstoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;


        private readonly IPasswordHasher<User> _passwordHasher;


        // O construtor PRECISA receber o passwordHasher
        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher) // <-- PARÂMETRO ADICIONADO AQUI
        {
            _context = context;
            _passwordHasher = passwordHasher; // Agora esta linha funciona
        }

        // GET: api/users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}", Name = "GetUser")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);

            if (user == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado.");
            }
            return user;
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CriarUser([FromBody] User novoUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- CORREÇÃO DE SEGURANÇA ---
            // Esta linha agora funciona
            novoUser.Senha = _passwordHasher.HashPassword(novoUser, novoUser.Senha);
            // --- FIM DA CORREÇÃO DE SEGURANÇA ---

            novoUser.Status = true;

            await _context.Usuarios.AddAsync(novoUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = novoUser.Id }, novoUser);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> AtualizarUser(int id, [FromBody] User userAtualizado)
        {
            if (id != userAtualizado.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID do usuário fornecido.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- CONSIDERAÇÃO DE SEGURANÇA ---
            // (Lógica de hashing de senha na atualização precisará ser adicionada aqui no futuro)
            // --- FIM DA CONSIDERAÇÃO ---

            _context.Entry(userAtualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(e => e.Id == id))
                {
                    return NotFound($"Usuário com ID {id} não encontrado para atualização.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletarUser(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado para exclusão.");
            }

            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}