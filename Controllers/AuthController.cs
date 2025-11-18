using ControleEstoque.Api.Data;
using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        
        private readonly IPasswordHasher<User> _passwordHasher;
      

        // O construtor PRECISA receber o passwordHasher
        public AuthController(AppDbContext context, IConfiguration configuration, IPasswordHasher<User> passwordHasher) // <-- PARÂMETRO ADICIONADO AQUI
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher; // Agora esta linha funciona
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _context.Usuarios
                                     .FirstOrDefaultAsync(u => u.Email == loginModel.Email);

            if (user == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            if (user.Status == false)
            {
                return Unauthorized("Usuário está inativo. Contate o administrador.");
            }

            // --- CORREÇÃO DE SEGURANÇA ---
            // Esta linha agora funciona
            var result = _passwordHasher.VerifyHashedPassword(user, user.Senha, loginModel.Senha);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Email ou senha inválidos.");
            }
            // --- FIM DA CORREÇÃO DE SEGURANÇA ---

            var token = GenerateJwtToken(user);
            return Ok(new { token = token, role = user.Tipo });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("tipo", user.Tipo)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}