using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ControleEstoque.Api.Controllers
{
    // DTO simples para receber os dados de login
    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // 1. Busca o usuário pelo email no banco
            var user = await _context.Usuarios.Find(u => u.Email == loginModel.Email).FirstOrDefaultAsync();

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

            // 3. Gera o Token JWT
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
                    new Claim(ClaimTypes.NameIdentifier, user.Id!.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    // Você pode adicionar outras "claims" (informações) aqui, como o tipo/role do usuário
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