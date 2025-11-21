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
    [Authorize] // Deixado comentado para você poder criar o primeiro Admin
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

        public async Task<ActionResult<List<User>>> GetUsers()
        {
            // ATENÇÃO: Retorna usuários COM senha (inseguro). Idealmente usar um DTO.
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}", Name = "GetUser")]

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

            // --- INÍCIO DA CORREÇÃO ---

            // 1. Buscamos o usuário que já existe no banco de dados
            var userDoBanco = await _context.Usuarios.FindAsync(id);

            if (userDoBanco == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado para atualização.");
            }

            // 2. Atualizamos apenas os campos de dados (e não o Status)
            userDoBanco.Nome = userAtualizado.Nome;
            userDoBanco.Email = userAtualizado.Email;
            userDoBanco.Tipo = userAtualizado.Tipo;

            // O 'userDoBanco.Status' (que é 'true') é preservado, pois não mexemos nele.
            // O 'userAtualizado.Status' (que é 'false') é ignorado.

            // 3. (BÓNUS): Implementa a lógica de senha que estava faltando
            // Se o frontend enviou uma senha nova (não nula ou vazia)...
            if (!string.IsNullOrWhiteSpace(userAtualizado.Senha))
            {
                // ...nós fazemos o hash dela e atualizamos no banco.
                userDoBanco.Senha = _passwordHasher.HashPassword(userDoBanco, userAtualizado.Senha);
            }
            // Se a senha veio vazia, a senha antiga (userDoBanco.Senha) é mantida.

            // 4. Em vez de 'userAtualizado', salvamos o 'userDoBanco' modificado
            try
            {
                // O EF Core já está rastreando 'userDoBanco', então só precisamos salvar.
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

            // --- FIM DA CORREÇÃO ---

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarUser(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado para exclusão.");
            }

            // --- CORREÇÃO (Soft Delete) ---
            // Em vez de remover, definimos o status como inativo
            user.Status = false;
            // --- FIM DA CORREÇÃO ---

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
