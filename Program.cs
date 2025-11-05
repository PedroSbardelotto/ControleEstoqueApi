using ControleEstoque.Api.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using ControleEstoque.Api.Models;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// --- INÍCIO CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// --- FIM CORS ---

// --- Configuração do Banco de Dados ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
// --- FIM Configuração do Banco de Dados ---


// --- Configuração de Autenticação JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
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
builder.Services.AddSwaggerGen(options =>
{
    // ... (Toda a sua configuração AddSecurityDefinition e AddSecurityRequirement vai aqui) ...
    // (Omitido para encurtar, mas mantenha o seu código de Swagger que já funciona)
});
// --- FIM Configuração do Swagger ---


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// --- INÍCIO: MIGRAÇÃO E SEED AUTOMÁTICOS ---
// Esta função será chamada para preparar o banco de dados
await ApplyMigrationAndSeedDataAsync(app);

async Task ApplyMigrationAndSeedDataAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            // --- ETAPA 1: APLICAR MIGRAÇÕES ---
            logger.LogInformation("Verificando e aplicando migrações do banco de dados...");
            await context.Database.MigrateAsync(); // <-- ISSO RODA O EQUIVALENTE A 'dotnet ef database update'
            logger.LogInformation("Migrações aplicadas com sucesso.");

            // --- ETAPA 2: SEMEAR USUÁRIO ADMIN ---
            if (!await context.Usuarios.AnyAsync())
            {
                logger.LogInformation("Banco de dados de usuários vazio. Criando usuário Admin padrão...");

                var adminEmail = configuration["ADMIN_EMAIL"];
                var adminPassword = configuration["ADMIN_PASSWORD"];

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    logger.LogError("ADMIN_EMAIL ou ADMIN_PASSWORD não configurados. Usuário Admin não pode ser criado.");
                }
                else
                {
                    var adminUser = new User
                    {
                        Nome = "Administrador",
                        Email = adminEmail,
                        Tipo = "Admin",
                        Status = true
                    };
                    adminUser.Senha = passwordHasher.HashPassword(adminUser, adminPassword);

                    await context.Usuarios.AddAsync(adminUser);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Usuário Admin padrão criado com sucesso.");
                }
            }
            else
            {
                logger.LogInformation("Banco de dados já contém usuários. Seed do Admin ignorado.");
            }
        }
        catch (Exception ex)
        {
            // Loga o erro se a migração ou o seed falharem
            logger.LogError(ex, "Ocorreu um erro ao tentar migrar ou semear o banco de dados.");
            // (Em um cenário de produção real, você pode querer decidir se a app deve parar aqui)
        }
    }
}
// --- FIM: MIGRAÇÃO E SEED AUTOMÁTICOS ---


// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();