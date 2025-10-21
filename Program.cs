using ControleEstoque.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // <-- ADICIONADO para EF Core
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- IN�CIO CORS ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // IMPORTANTE: Substitua pelos endere�os do seu front-end
                          // quando voc� o tiver. Pode adicionar quantos forem necess�rios.
                          policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM CORS ---

// --- Configura��o do Banco de Dados ---
// REMOVIDO: Configura��o do MongoDB
// builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
// builder.Services.AddSingleton<MongoDbContext>();

// ADICIONADO: Configura��o do SQL Server com EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// --- FIM Configura��o do Banco de Dados ---


// --- Configura��o de Autentica��o JWT ---
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
        ValidateIssuer = false, // Em cen�rios mais complexos, pode validar o emissor
        ValidateAudience = false // Em cen�rios mais complexos, pode validar a audi�ncia
    };
});
// --- FIM Configura��o de Autentica��o JWT ---


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adiciona o middleware do CORS ao pipeline.
// A ordem aqui � importante!
app.UseCors(MyAllowSpecificOrigins);

// --- Configura��o de Autentica��o e Autoriza��o no Pipeline ---
// A ordem correta � Authentication -> Authorization
app.UseAuthentication(); // Garante que essa linha esteja presente
app.UseAuthorization();
// --- FIM Configura��o de Autentica��o e Autoriza��o ---


app.MapControllers();

app.Run();