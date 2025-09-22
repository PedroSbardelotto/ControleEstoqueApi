using ControleEstoque.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// --- IN�CIO DA CONFIGURA��O DO CORS ---

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

// --- FIM DA CONFIGURA��O DO CORS ---


// Adiciona os servi�os ao cont�iner.

// Configura��o do MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();