using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- ADICIONADO
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    // DTO simples para receber os dados de login (não muda)
    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context; // <-- ALTERADO para AppDbContext
        private readonly IConfiguration _configuration;

        // Construtor alterado para injetar AppDbContext
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // 1. Busca o usuário pelo email no banco usando LINQ
            var user = await _context.Usuarios
                                     .FirstOrDefaultAsync(u => u.Email == loginModel.Email); // <-- ALTERADO para LINQ

            // 2. Valida as credenciais
            if (user == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // ATENÇÃO: Falha de segurança GRAVE! Apenas para fins didáticos.
            // O próximo passo DEVE ser implementar hashing de senha.
            if (user.Senha != loginModel.Senha)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // 3. Gera o Token JWT (não muda, mas Id agora é int)
            var token = GenerateJwtToken(user);

            // 4. Retorna o token
            return Ok(new { token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Id agora é int, então ToString() funciona diretamente
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Tipo)
                }),
                Expires = DateTime.UtcNow.AddHours(8), // Tempo de validade do token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}