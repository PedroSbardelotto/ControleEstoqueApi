using ControleEstoque.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Mapeia as configurações do appsettings.json para a nossa classe MongoDbSettings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// 2. Adiciona nosso MongoDbContext como um serviço Singleton (uma única instância para toda a aplicação)
builder.Services.AddSingleton<MongoDbContext>();



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
