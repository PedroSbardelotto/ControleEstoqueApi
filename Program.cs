using ControleEstoque.Api.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using ControleEstoque.Api.Models;
// Adicionar este using para OpenApiReference
using Microsoft.OpenApi.Models;
// ADICIONAR ESTE USING PARA O POSTGRESQL
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// --- INÍCIO CORS ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    // Sua política padrão com AllowAnyOrigin() já funciona perfeitamente para o Render.
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()   // Permite qualquer origem
                  .AllowAnyHeader()   // Permite cabeçalhos
                  .AllowAnyMethod();  // Permite métodos
        });
});
// --- FIM CORS ---

// --- Configuração do Banco de Dados ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    // options.UseSqlServer(connectionString)); // <-- REMOVIDO
    options.UseNpgsql(connectionString)); // <-- ADICIONADO (Troca para PostgreSQL)
// --- FIM Configuração do Banco de Dados ---


// --- Configuração de Autenticação JWT ---
// (Esta parte configura COMO a API valida os tokens)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em produção, considere usar true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
// --- FIM Configuração de Autenticação JWT ---


// --- Configuração do Swagger para entender JWT ---
// (Esta parte configura COMO o Swagger mostra o botão Authorize)
builder.Services.AddSwaggerGen(options =>
{
    // 1. Define o esquema de segurança
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Usar ApiKey para o header Authorization
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira 'Bearer' [espaço] e então o seu token no campo abaixo.\n\nExemplo: \"Bearer 12345abcdef\""
    });

    // 2. Adiciona a exigência de autorização (CORRIGIDO)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Tipo da referência
                    Id = "Bearer" // ID do SecurityDefinition
                },
                Scheme = "oauth2", // Apenas informativo
                Name = "Bearer",   // Apenas informativo
                In = ParameterLocation.Header // Apenas informativo
            },
            new List<string>() // Lista de escopos (vazia para JWT)
        }
    });
});
// --- FIM Configuração do Swagger ---


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Esta opção diz ao serializador para ignorar ciclos
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usa a política de CORS padrão (AllowAnyOrigin)
app.UseCors();

// Ordem correta: Autenticação primeiro, depois Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();