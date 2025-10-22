using ControleEstoque.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// Adicionar este using para OpenApiReference
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- IN�CIO CORS ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM CORS ---

// --- Configura��o do Banco de Dados ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// --- FIM Configura��o do Banco de Dados ---


// --- Configura��o de Autentica��o JWT ---
// (Esta parte configura COMO a API valida os tokens)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em produ��o, considere usar true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
// --- FIM Configura��o de Autentica��o JWT ---


// --- Configura��o do Swagger para entender JWT ---
// (Esta parte configura COMO o Swagger mostra o bot�o Authorize)
builder.Services.AddSwaggerGen(options =>
{
    // 1. Define o esquema de seguran�a
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Usar ApiKey para o header Authorization
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira 'Bearer' [espa�o] e ent�o o seu token no campo abaixo.\n\nExemplo: \"Bearer 12345abcdef\""
    });

    // 2. Adiciona a exig�ncia de autoriza��o (CORRIGIDO)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                {
                    Type = ReferenceType.SecurityScheme, // Tipo da refer�ncia
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
// --- FIM Configura��o do Swagger ---


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configura o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

// Ordem correta: Autentica��o primeiro, depois Autoriza��o
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();