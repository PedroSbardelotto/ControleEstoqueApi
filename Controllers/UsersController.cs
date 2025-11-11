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
    // [Authorize] // Deixado comentado para você poder criar o primeiro Admin
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/users
        [HttpGet]
        [Authorize] // Protegendo o GET geral
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            // ATENÇÃO: Retorna usuários COM senha (inseguro). Idealmente usar um DTO.
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}", Name = "GetUser")]
        [Authorize] // Protegendo o GET por ID
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);

            if (user == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado.");
            }
            // ATENÇÃO: Retorna usuário COM senha (inseguro). Idealmente usar um DTO.
            return user;
        }

        // POST: api/users
        [HttpPost]
        // [Authorize(Roles = "Admin")] // Mantenha comentado por enquanto
        public async Task<ActionResult<User>> CriarUser([FromBody] User novoUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- HASHING DE SENHA ---
            novoUser.Senha = _passwordHasher.HashPassword(novoUser, novoUser.Senha);

            // Garante que o novo usuário seja 'Ativo'
            novoUser.Status = true;

            await _context.Usuarios.AddAsync(novoUser);
            await _context.SaveChangesAsync();

            // ATENÇÃO: Retorna usuário COM senha (inseguro). Idealmente usar um DTO.
            return CreatedAtAction(nameof(GetUser), new { id = novoUser.Id }, novoUser);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize] // Protegendo a atualização
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

            // (Lógica de hashing de senha na atualização precisará ser adicionada aqui no futuro)

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
        [Authorize(Roles = "Admin")] // Somente Admins podem deletar usuários
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